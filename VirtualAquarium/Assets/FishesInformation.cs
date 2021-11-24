using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualAquarium
{
    public class FishesInformation : MonoBehaviour
    {
        public GameObject fishInformationPrefab;
        private List<GameObject> list;
        public GameObject fishesInformation;
        public Material selectFishMaterial;
        private RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = gameObject.GetComponent<RectTransform>();
                return rectTransform;
            }
            set => rectTransform = value;
        }

        public FishesInformation()
        {
            list = new List<GameObject>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        public void AddFishInformation(Fish fish)
        {
            RectTransform = gameObject.GetComponent<RectTransform>();
            GameObject fishInformation = Instantiate(fishInformationPrefab, transform);
            RectTransform rectTransformFish = fishInformation.GetComponent<RectTransform>();
            rectTransformFish.anchoredPosition = new Vector2(0, -10 - (list.Count * rectTransformFish.sizeDelta.y));
            list.Add(fishInformation);
            fishInformation.GetComponent<FishInformation>().Fish = fish;
            RectTransform.sizeDelta.Set(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y + rectTransformFish.sizeDelta.y);
        }

        public void RemoveFishInformation(GameObject gameObject)
        {
            if (list.Contains(gameObject))
            {
                list.Remove(gameObject);
                Destroy(gameObject);
                RearrangeFishInformation();
            }
        }

        public void RearrangeFishInformation()
        {
            RectTransform.sizeDelta.Set(RectTransform.sizeDelta.x, 0);
            for (int i = 0; i < list.Count; i++)
            {
                RectTransform rectTransformFish = list[i].GetComponent<RectTransform>();
                rectTransformFish.anchoredPosition = new Vector2(0, -10 - (i * rectTransformFish.sizeDelta.y));
                RectTransform.sizeDelta.Set(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y + rectTransformFish.sizeDelta.y);
            }
        }
    }
}