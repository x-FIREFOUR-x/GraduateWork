using UnityEngine;


namespace TowerDefense.Main.Units
{
    public class UnitCorpse : MonoBehaviour
    {
        private const float gravity = 18f;
        private const float rollAngle = 70f;
        private const float rollSpeed = 1.6f;
        private const float lieTime = 1f;
        private const float sinkSpeed = 1.2f;
        private const float sinkDepth = 2.5f;

        private float restHeight;
        private float fallSpeed;
        private float rollProgress;
        private float timeLying;
        private float sunk;
        private bool isLying;
        private Quaternion startRotation;
        private Quaternion restRotation;


        public void Fall(float groundHeight, float bodyThickness, float rollSide)
        {
            restHeight = groundHeight + bodyThickness;
            startRotation = transform.rotation;
            restRotation = startRotation * Quaternion.Euler(8f, 0f, rollAngle * rollSide);
        }

        void Update()
        {
            if (!isLying)
                Drop();
            else if (timeLying < lieTime)
                timeLying += Time.deltaTime;
            else
                Sink();
        }

        private void Drop()
        {
            fallSpeed += gravity * Time.deltaTime;
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            rollProgress = Mathf.Min(1f, rollProgress + Time.deltaTime * rollSpeed);
            transform.rotation = Quaternion.Slerp(startRotation, restRotation, Mathf.SmoothStep(0f, 1f, rollProgress));

            if (transform.position.y <= restHeight)
            {
                transform.position = new Vector3(transform.position.x, restHeight, transform.position.z);
                transform.rotation = restRotation;
                isLying = true;
            }
        }

        private void Sink()
        {
            float step = sinkSpeed * Time.deltaTime;
            transform.position += Vector3.down * step;
            sunk += step;

            if (sunk >= sinkDepth)
                Destroy(gameObject);
        }
    }

}
