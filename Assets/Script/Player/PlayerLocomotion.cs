using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LM
{
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject; // ��������Transform
        InputHandler inputHandler; // ���봦����
        public Vector3 moveDirection; // �ƶ���������

        [HideInInspector]
        public Transform myTransform; // ��ǰ�����Transform��������Inspector�У�
        [HideInInspector]
        public AnimatorHandler animationHandler; // ����������

        public new Rigidbody rigidbody; // �������
        public GameObject normalCamera; // ��ͨ�������

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        float minimumDistanceNeedToBeginFall = 1f;
        [SerializeField]
        float groundDirectionRayDistance = 0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")] // ��Inspector����ʾһ������Ϊ"Stats"�ķ���
        [SerializeField]
        float movementSpeed = 5; // �ƶ��ٶ�
        [SerializeField]
        float walkingSpeed = 1; // �����ٶ�
        [SerializeField]
        float sprintSpeed = 7; // ����ٶ�
        [SerializeField]
        float rotationSpeed = 10; // ��ת�ٶ�
        [SerializeField]
        float fallingSpeed = 18;

        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animationHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animationHandler.Initialize();

            playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }
        #region Movement
        Vector3 normalVector;
        Vector3 targetPosition;
        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero; // Ŀ�귽��������ʼ��Ϊ������
            float moveOverride = inputHandler.moveAmount; // �ƶ�������
            targetDir = cameraObject.forward * inputHandler.vertical; // ���������ǰ������ʹ�ֱ��������Ŀ�귽��
            targetDir += cameraObject.right * inputHandler.horizontal; // ����������ҷ����ˮƽ�������Ŀ�귽��

            targetDir.Normalize(); // ��һ��Ŀ�귽������
            targetDir.y = 0; // ��y�������Ϊ0������ƽ���ƶ�

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward; // ���Ŀ�귽��Ϊ����������ʹ�õ�ǰ�����ǰ����

            float rs = rotationSpeed; // ��ת�ٶ�
            Quaternion tr = Quaternion.LookRotation(targetDir); // ��Ŀ�귽��ת��Ϊ��ת
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta); // ʹ�����β�ֵƽ������ת��Ŀ�귽��

            myTransform.rotation = targetRotation; // Ӧ����ת����ǰ�����Rotation
        }
        public void HandleMovement(float delta)
        {
            if (inputHandler.rollFlag)
                return;

            if (playerManager.isInteracting)
                return;

            moveDirection = cameraObject.forward * inputHandler.vertical; // �����ƶ�����ǰ��
            moveDirection += cameraObject.right * inputHandler.horizontal; // ����ƶ��������ң�
            moveDirection.Normalize(); // ��һ���ƶ�����

            float speed = movementSpeed; // �ƶ��ٶ�

            if (inputHandler.sprintFlag && inputHandler.moveAmount > 0.5)
            {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                if (inputHandler.moveAmount < 0.5)
                {
                    moveDirection *= walkingSpeed;
                    playerManager.isSprinting = false;
                }
                else
                {
                    moveDirection *= speed; // �����ٶȵ����ƶ�����
                    playerManager.isSprinting = false;
                }

            }

            Vector3 projectVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector); // ���ƶ�����ͶӰ��������������ƽ����
            rigidbody.velocity = projectVelocity; // Ӧ���ƶ��ٶȵ�������ٶ�����

            animationHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting); // ���¶�������

            if (animationHandler.canRotate) // ���������ת
            {
                HandleRotation(delta); // ������ת
            }
        }
        public void HandleRollingAndSprinting(float delta)
        {
            if (animationHandler.anim.GetBool("isInteracting")) return;
            // ������봦������е�rollFlagΪ��
            if (inputHandler.rollFlag)
            {
                // �����ƶ�����Ϊ����ͷ��ǰ�����Դ�ֱ���� + ����ͷ���ҷ�����ˮƽ����
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;

                // ����ƶ������� 0
                if (inputHandler.moveAmount > 0)
                {
                    // ����Ŀ�궯��"Rolling"��ѭ��Ϊ��
                    animationHandler.PlayTargetAnimation("Rolling", true);

                    // ���ƶ������y����Ϊ0��ʹ��ɫֻ��ˮƽ���Ϲ���
                    moveDirection.y = 0;

                    // ����һ������moveDirection��Quaternion����rollRotation��������ɫ����ת��ΪrollRotation
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    // ����Ŀ�궯��"Rolling"��ѭ��Ϊ��
                    animationHandler.PlayTargetAnimation("Rolling", false);
                }
            }
        }
        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;
            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }
            if (playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 5f);
            }
            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;

            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeedToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeedToBeginFall, ignoreForGroundCheck))
            {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                playerManager.isGrounded = true;
                targetPosition.y = tp.y;
                if (playerManager.isInAir)
                {
                    if (inAirTimer > 0.5f)
                    {
                        Debug.Log("You were in the air for " + inAirTimer);
                        animationHandler.PlayTargetAnimation("Land", true);
                    }
                    else
                    {
                        animationHandler.PlayTargetAnimation("Empty", false);
                        inAirTimer = 0;
                    }
                    playerManager.isInAir = false;
                }
            }
            else
            {
                if (playerManager.isGrounded)
                {
                    playerManager.isGrounded = false;
                }
                if (playerManager.isInAir == false)
                {
                    if (playerManager.isInteracting == false)
                    {
                        animationHandler.PlayTargetAnimation("Falling", true);
                    }
                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    playerManager.isInAir = true;
                }
            }
            if (playerManager.isInteracting || inputHandler.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime/0.1f);
            }
            else
            {
                myTransform.position = targetPosition;
            }

        }
        #endregion
    }

}
