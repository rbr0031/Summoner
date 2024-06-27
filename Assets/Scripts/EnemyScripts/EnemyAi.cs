using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using JetBrains.Annotations;

public class EnemyAi : NetworkBehaviour
{
        public NavMeshAgent agent;
        public float chaseDistance = 10f;
        public float attackDistance = 2f;
        public float patrolRadius = 5f;
        public float attackCooldown = 1f;
        private Vector3 _origin;
        private bool _isAttacking;
        private bool _wasAttacked;

        public BehaviorTree _tree;
        private List<Vector3> _patrolPoints;
        private int _currentPatrolIndex;
        private GameObject _currentTarget;

        private float _updateTargetInterval = 4f;
        private float _timeSinceLastTargetUpdate = 0f;

        float _timeSinceLastAttack = 0f;
        const float _maxTimeWithoutAttack = 5f;
        bool _isChasing = false;



    private void Start()
        {
            _origin = transform.position;
            GeneratePatrolPoints();

            // Build behavior tree
            BuildBehaviorTree();
        }
        private void Update()
        {
            if (!IsServer) return;

            _timeSinceLastTargetUpdate += Time.deltaTime;
            if (_timeSinceLastTargetUpdate >= _updateTargetInterval)
            {
                UpdateCurrentTarget();
                _timeSinceLastTargetUpdate = 0f;
            }

            if (_isChasing)
            {
                _timeSinceLastAttack += Time.deltaTime;

            } else if (_timeSinceLastAttack >= _maxTimeWithoutAttack)
            {
                StartCoroutine(WaitForSecondsBeforeChasing());
            }

            _tree.Tick();

            UpdateEnemyPositionClientRpc(transform.position);

        }
        private void BuildBehaviorTree()
        {
            _tree = new BehaviorTreeBuilder(gameObject)
                .Selector()
                    .Sequence("Attack Player")
                        .Condition("Has Target", () => _currentTarget != null)
                        .Condition("Target In Attack Range", () => Vector3.Distance(_currentTarget.transform.position, transform.position) <= attackDistance)
                        .Condition("Not Attacking", () => !_isAttacking)
                        .Do("Attack", () => {
                            StartCoroutine(AttackPlayer());
                            _timeSinceLastAttack = 0f; 
                            return TaskStatus.Success;
                        })
                    .End()
                    .Sequence("Chase Player")
                        .Condition("Has Target", () => _currentTarget != null)
                        .Condition("Target In Range or was attacked", () => _wasAttacked || Vector3.Distance(_currentTarget.transform.position, transform.position) <= chaseDistance)
                        .Condition("Within Attack Time Limit", () => _timeSinceLastAttack <= _maxTimeWithoutAttack)
                        .Do("Move To Target", () => {
                            _isChasing = true;
                            agent.SetDestination(_currentTarget.transform.position);
                            return TaskStatus.Success;
                        })
                    .End()
                    .Sequence("Patrol")
                        .Do("Move To Patrol Point", () => {
                            _isChasing = false;
                            if (Vector3.Distance(transform.position, _patrolPoints[_currentPatrolIndex]) <= 1)
                            {
                                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
                            }
                            agent.SetDestination(_patrolPoints[_currentPatrolIndex]);
                            return TaskStatus.Success;
                        })
                    .End()
                .Build();
        }

        private void GeneratePatrolPoints()
        {
            _patrolPoints = new List<Vector3>();
            for (int i = 0; i < 5; i++)
            {
                Vector3 randomPoint = _origin + Random.insideUnitSphere * patrolRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, 1))
                {
                    _patrolPoints.Add(hit.position);
                }
            }
            _currentPatrolIndex = 0;
        }

        private void UpdateCurrentTarget()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject closestPlayer = null;
            float minDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(player.transform.position, currentPosition);
                if (distance < minDistance)
                {
                    closestPlayer = player;
                    minDistance = distance;
                }
            }

            if (closestPlayer != null)
            {
                _currentTarget = closestPlayer;
            }

        }

        public void SetCurrentTarget(NetworkObjectReference attackerId)
        {
            
            if (attackerId.TryGet(out NetworkObject attackerObject))
            {
                _currentTarget = attackerObject.gameObject;
                _wasAttacked = true;
                Debug.Log($"Set new target: {attackerObject}");
            }
            else
            {
                Debug.LogError($"Attacker with ID {attackerId} not found in spawned objects.");
            }
        }

        private IEnumerator WaitForSecondsBeforeChasing()
        {
            yield return new WaitForSeconds(3);

            _timeSinceLastAttack = 0f;
            _currentTarget = null;
        }

        private IEnumerator AttackPlayer()
        {
            _isAttacking = true;
            Debug.Log("Attacking player: " + _currentTarget.name);
            yield return new WaitForSeconds(attackCooldown);
            _isAttacking = false;
        }

        [ClientRpc]
        private void UpdateEnemyPositionClientRpc(Vector3 newPosition)
        {
            transform.position = newPosition;
        }
}