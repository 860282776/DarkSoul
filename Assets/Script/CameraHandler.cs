using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LM
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform targetTransform; // Ŀ�������Transform
        public Transform cameraTransform; // �����Transform
        public Transform cameraPivotTransform; // ��������Transform
        private Transform myTransform; // ��ǰ�����Transform
        private Vector3 cameraTransformPosition; // �����λ������
        private LayerMask ignoreLayers; // ���ԵĲ�
        private Vector3 cameraFollowVelocity = Vector3.zero;

        public static CameraHandler singleton; // ��̬����

        public float lookSpeed = 0.1f; // �ӽ���ת�ٶ�
        public float followSpeed = 0.1f; // �����ٶ�
        public float pivotSpeed = 0.03f; // �����ٶ�

        private float targetPosition;
        private float defaultPosition; // Ĭ��λ��
        private float lookAngle; // �ӽǽǶ�
        private float pivotAngle; // ����Ƕ�
        public float minimumPivot = -35; // ������С�Ƕ�
        public float maximumPivot = 35; // �������Ƕ�

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
            //�ع�ǰ����
            //Vector3 targetPosition = Vector3.Lerp(myTransform.position, targetTransform.position, delta / followSpeed);
            
            // ʹ��SmoothDamp����ƽ���ؽ�����ӵ�ǰλ��(myTransform.position)�ƶ���Ŀ��λ��(targetTransform.position)������ʱ����(delta)�͸����ٶ�(followSpeed)�����ٶ�
            Vector3 targetPosition = Vector3.SmoothDamp
                            (myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            // �������λ�õ�Ŀ��λ��
            myTransform.position = targetPosition;

            HandleCameraCollision(delta);
        }
        public void HandleCameraRotation(float delta,float mouseXInput,float mouseYInput)
        {
            lookAngle += (mouseXInput * lookSpeed) / delta; // �������ˮƽ��������ӽǽǶ�
            pivotAngle -= (mouseYInput * pivotSpeed) / delta; // ������괹ֱ�����������Ƕ�
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot); // ��������Ƕ�����С�����ֵ֮��

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation; // Ӧ���ӽ���ת����ǰ�����Rotation

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation; // Ӧ��������ת���������ı���Rotation
        }
        private void HandleCameraCollision(float delta)
        {
            // ��Ŀ��λ������ΪĬ��λ��
            targetPosition = defaultPosition;

            // ��������
            RaycastHit hit;
            Vector3 direction = cameraTransform.position - cameraPivotTransform.position;
            direction.Normalize();

            // �����ײ
            if (Physics.SphereCast(cameraPivotTransform.position, cameraSphereRadius, direction, out hit, Mathf.Abs(targetPosition), ignoreLayers))
            {
                // �����������ײ��ľ���
                float dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
                // ����Ŀ��λ�ã�ȷ�����Զ���ϰ���
                targetPosition = -(dis - cameraCollisionOffset);
            }

            // ����Ƿ������С��ײƫ��ֵ
            if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
            {
                // ��Ŀ��λ����Ϊ��С��ײƫ��ֵ�ĸ�ֵ
                targetPosition = -minimumCollisionOffset;
            }

            // ʹ�����Բ�ֵƽ�����ƶ����λ��
            cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / 0.2f);
            cameraTransform.localPosition = cameraTransformPosition;
        }
    }
}

