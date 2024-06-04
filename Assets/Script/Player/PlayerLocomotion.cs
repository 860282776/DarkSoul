using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LM
{
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerManager playerManager;
        Transform cameraObject; // 相机物体的Transform
        InputHandler inputHandler; // 输入处理器
        public Vector3 moveDirection; // 移动方向向量

        [HideInInspector]
        public Transform myTransform; // 当前物体的Transform（隐藏在Inspector中）
        [HideInInspector]
        public AnimatorHandler animationHandler; // 动画处理器

        public new Rigidbody rigidbody; // 刚体组件
        public GameObject normalCamera; // 普通相机物体

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        float minimumDistanceNeedToBeginFall = 1f;
        [SerializeField]
        float groundDirectionRayDistance = 0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Movement Stats")] // 在Inspector中显示一个标题为"Stats"的分组
        [SerializeField]
        float movementSpeed = 5; // 移动速度
        [SerializeField]
        float walkingSpeed = 1; // 行走速度
        [SerializeField]
        float sprintSpeed = 7; // 冲刺速度
        [SerializeField]
        float rotationSpeed = 10; // 旋转速度
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
            Vector3 targetDir = Vector3.zero; // 目标方向向量初始化为零向量
            float moveOverride = inputHandler.moveAmount; // 移动输入量
            targetDir = cameraObject.forward * inputHandler.vertical; // 根据摄像机前方方向和垂直输入设置目标方向
            targetDir += cameraObject.right * inputHandler.horizontal; // 根据摄像机右方向和水平输入调整目标方向

            targetDir.Normalize(); // 归一化目标方向向量
            targetDir.y = 0; // 将y轴分量置为0，保持平面移动

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward; // 如果目标方向为零向量，则使用当前物体的前方向

            float rs = rotationSpeed; // 旋转速度
            Quaternion tr = Quaternion.LookRotation(targetDir); // 将目标方向转换为旋转
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta); // 使用球形插值平滑地旋转到目标方向

            myTransform.rotation = targetRotation; // 应用旋转到当前物体的Rotation
        }
        public void HandleMovement(float delta)
        {
            if (inputHandler.rollFlag)
                return;

            if (playerManager.isInteracting)
                return;

            moveDirection = cameraObject.forward * inputHandler.vertical; // 计算移动方向（前后）
            moveDirection += cameraObject.right * inputHandler.horizontal; // 添加移动方向（左右）
            moveDirection.Normalize(); // 归一化移动方向

            float speed = movementSpeed; // 移动速度

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
                    moveDirection *= speed; // 根据速度调整移动方向
                    playerManager.isSprinting = false;
                }

            }

            Vector3 projectVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector); // 将移动方向投影到法线向量所在平面上
            rigidbody.velocity = projectVelocity; // 应用移动速度到刚体的速度属性

            animationHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting); // 更新动画参数

            if (animationHandler.canRotate) // 如果允许旋转
            {
                HandleRotation(delta); // 处理旋转
            }
        }
        public void HandleRollingAndSprinting(float delta)
        {
            if (animationHandler.anim.GetBool("isInteracting")) return;
            // 如果输入处理程序中的rollFlag为真
            if (inputHandler.rollFlag)
            {
                // 计算移动方向为摄像头的前方乘以垂直输入 + 摄像头的右方乘以水平输入
                moveDirection = cameraObject.forward * inputHandler.vertical;
                moveDirection += cameraObject.right * inputHandler.horizontal;

                // 如果移动量大于 0
                if (inputHandler.moveAmount > 0)
                {
                    // 播放目标动画"Rolling"，循环为真
                    animationHandler.PlayTargetAnimation("Rolling", true);

                    // 将移动方向的y轴设为0，使角色只在水平面上滚动
                    moveDirection.y = 0;

                    // 创建一个朝向moveDirection的Quaternion对象rollRotation，并将角色的旋转设为rollRotation
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    // 播放目标动画"Rolling"，循环为假
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
