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
        Vector3 moveDirection; // �ƶ���������

        [HideInInspector]
        public Transform myTransform; // ��ǰ�����Transform��������Inspector�У�
        [HideInInspector]
        public AnimatorHandler animationHandler; // ����������

        public new Rigidbody rigidbody; // �������
        public GameObject normalCamera; // ��ͨ�������

        [Header("Movement Stats")] // ��Inspector����ʾһ������Ϊ"Stats"�ķ���
        [SerializeField]
        float movementSpeed = 5; // �ƶ��ٶ�
        [SerializeField]
        float sprintSpeed = 7; // ����ٶ�
        [SerializeField]
        float rotationSpeed = 10; // ��ת�ٶ�

        void Start()
        {
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animationHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animationHandler.Initialize();
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

            moveDirection = cameraObject.forward * inputHandler.vertical; // �����ƶ�����ǰ��
            moveDirection += cameraObject.right * inputHandler.horizontal; // ����ƶ��������ң�
            moveDirection.Normalize(); // ��һ���ƶ�����

            float speed = movementSpeed; // �ƶ��ٶ�

            if (inputHandler.sprintFlag)
            {
                speed = sprintSpeed;
                playerManager.isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                moveDirection *= speed; // �����ٶȵ����ƶ�����
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
        #endregion
    }

}
