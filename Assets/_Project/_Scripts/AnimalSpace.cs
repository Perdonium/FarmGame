using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;

namespace FarmGame
{
    public class AnimalSpace : MonoBehaviour
    {

        [SerializeField]
        BoxCollider2D movingSpace;
        [SerializeField]
        Transform animal;
        [SerializeField]
        Transform basePosition;

        bool goToSleep = false;

        const float animalSpeed = 1f;

        // Start is called before the first frame update
        void Start()
        {
            MessageKit<bool>.addObserver(Messages.NightSwitch, (b) => ManageAnimal(b));

            ManageAnimal(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void ManageAnimal(bool night)
        {
            if (night)
            {
                Debug.Log("coucou");
                goToSleep = true;
            }
            else
            {

                animal.gameObject.SetActive(true);
                animal.position = basePosition.position;

                MoveToNextPosition();
            }
        }

        void MoveToNextPosition()
        {


            Vector3 nextPosition = GetRandomPositionWithinSpace();
            if (nextPosition.x < animal.position.x)
            {
                animal.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                animal.localScale = new Vector3(1, 1, 1);
            }

            if (goToSleep)
            {
                nextPosition = basePosition.position;
                animal.DOMove(basePosition.position, animalSpeed*Vector3.Distance(nextPosition, animal.position)).SetEase(Ease.Linear).OnComplete(() =>
                {
                    animal.gameObject.SetActive(false);
                    goToSleep = false;
                });
                return;
            }

            animal.DOMove(nextPosition, animalSpeed*Vector3.Distance(nextPosition, animal.position)).SetEase(Ease.Linear).OnComplete(() => MoveToNextPosition());
        }

        Vector3 GetRandomPositionWithinSpace()
        {
            Vector2 movingPos = (Vector2)movingSpace.transform.position + movingSpace.offset;
            float randomPosX = Random.Range(movingPos.x - movingSpace.size.x / 2, movingPos.x + movingSpace.size.x / 2);
            float randomPosY = Random.Range(movingPos.y - movingSpace.size.y / 2, movingPos.y + movingSpace.size.y / 2);
            return new Vector3(randomPosX, randomPosY, -1);

        }
    }

}