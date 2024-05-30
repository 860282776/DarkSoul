using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LM
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform targetTransform; // 目标物体的Transform
        public Transform cameraTransform; // 相机的Transform
        public Transform cameraPivotTransform; // 相机枢轴的Transform
        private Transform myTransform; // 当前物体的Transform
        private Vector3 cameraTransformPosition; // 相机的位置向量
        private LayerMask ignoreLayers; // 忽略的层
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton; // 静态单例

        public float lookSpeed = 0.1f; // 视角旋转速度
        public float followSpeed = 0.1f; // 跟随速度
        public float pivotSpeed = 0.03f; // 枢轴速度

        private float targetPosition;
        private float defaultPosition; // 默认位置
        private float lookAngle; // 视角角度
        private float pivotAngle; // 枢轴角度
        public float minimumPivot = -35; // 枢轴最小角度
        public float maximumPivot = 35; // 枢轴最大角度

        public float cameraSphereRadius = 0.2f;
        public float cameraCollisionOffset = 0.2f;
        public float minimumCollisionOffset = 0.2f;
        private void Awake()
        {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta)
        {
            //重构前代码
            //Vector3 targetPosition = Vector3.Lerp(myTransform.position, targetTransform.position, delta / followSpeed);
            
            // 使用SmoothDamp方法平滑地将相机从当前位置(myTransform.position)移动到目标位置(targetTransform.position)，根据时间间隔(delta)和跟随速度(followSpeed)调整速度
            Vector3 targetPosition = Vector3.SmoothDamp
                            (myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            // 更新相机位置到目标位置
            myTransform.position = targetPosition;

            HandleCameraCollision(delta);
        }
        public void HandleCameraRotation(float delta,float mouseXInput,float mouseYInput)
        {
            lookAngle += (mouseXInput * lookSpeed) / delta; // 根据鼠标水平输入调整视角角度
            pivotAngle -= (mouseYInput * pivotSpeed) / delta; // 根据鼠标垂直输入调整枢轴角度
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot); // 限制枢轴角度在最小和最大值之间

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation; // 应用视角旋转到当前物体的Rotation

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation; // 应用枢轴旋转到相机枢轴的本地Rotation
        }
        private void HandleCameraCollision(float delta)
        {
            // 将目标位置重置为默认位置
            targetPosition = defaultPosition;

            // 声明变量
            RaycastHit hit;
            Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
            direction.Normalize();

            // 检测碰撞
            if (Physics.SphereCast(cameraPivotTransform.position, cameraSphereRadius, direction, out hit, Mathf.Abs(targetPosition), ignoreLayers))
            {
                // 计算相机与碰撞点的距离
                float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
                // 调整目标位置，确保相机远离障碍物
                targetPosition = -(dis - cameraCollisionOffset);
            }

            // 检查是否低于最小碰撞偏移值
            if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
            {
                // 将目标位置设为最小碰撞偏移值的负值
                targetPosition = -minimumCollisionOffset;
            }

            // 使用线性插值平滑地移动相机位置
            cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.2f);
            cameraTransform.localPosition = cameraTransformPosition;
        }
    }
}

