using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31.MessageKit;
using DG.Tweening;

namespace FarmGame
{
    //Animate some animals that move around a given space
    public class AnimalSpace : MonoBehaviour
    {

        #region Properties

        [SerializeField]
        BoxCollider2D movingSpace; //BoxCollider2D seemed like the best way to represent the space
        [SerializeField]
        Transform animal;
        [SerializeField]
        Animator animalAnimator;
        [SerializeField]
        Transform basePosition; //The position at the start of the day

        bool goToSleep = false;
        bool moveToBase = false;
        const float animalSpeed = 1f;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            MessageKit<bool>.addObserver(Messages.NightSwitch, (b) => ManageAnimal(b));

            ManageAnimal(false);
        }

        void ManageAnimal(bool night)
        {
            if (night)
            {
                goToSleep = true;
            }
            else
            {
                //"Wake up" the animal
                animal.gameObject.SetActive(true);
                animal.position = basePosition.position;

                MoveToNextPosition();
            }
        }

        void MoveToNextPosition()
        {
            Vector3 nextPosition = GetRandomPositionWithinSpace();
            if (goToSleep)
            {
                nextPosition = basePosition.position;
                moveToBase = true;
            }

            //Makes the animal facing the right way
            if (nextPosition.x < animal.position.x)
            {
                animal.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                animal.localScale = new Vector3(1, 1, 1);
            }

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(Random.Range(0.5f,1f));
            seq.AppendCallback(() => animalAnimator.SetBool("moving",true));

            seq.Append(animal.DOMove(nextPosition, animalSpeed*Vector3.Distance(nextPosition, animal.position)).SetEase(Ease.Linear).OnComplete(() =>  {
                animalAnimator.SetBool("moving",false);
                if(moveToBase){
                    goToSleep = false;
                    moveToBase = false;
                    animal.gameObject.SetActive(false);
                } else {
                    MoveToNextPosition();
                }
            }));
            

        }

        //Generate a random position in the allowed space
        Vector3 GetRandomPositionWithinSpace()
        {
            Vector2 movingPos = (Vector2)movingSpace.transform.position + movingSpace.offset;
            float randomPosX = Random.Range(movingPos.x - movingSpace.size.x / 2, movingPos.x + movingSpace.size.x / 2);
            float randomPosY = Random.Range(movingPos.y - movingSpace.size.y / 2, movingPos.y + movingSpace.size.y / 2);
            return new Vector3(randomPosX, randomPosY, -1);

        }
    }

}