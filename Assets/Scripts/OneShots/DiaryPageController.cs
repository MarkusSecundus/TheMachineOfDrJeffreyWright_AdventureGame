using MarkusSecundus.PhysicsSwordfight.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OneShots
{
    internal class DiaryPageController : EnabilityChecked
    {
        [SerializeField] GameObject DiaryPrototypeRoot;

        protected override void Awake()
        {
            base.Awake();
            DiaryPrototypeRoot.SetActive(false);
        }

        GameObject _diaryInstance;
        protected override void OnEnable()
        {
            base.OnEnable();
            destroyDiaryInstance();
            createDiaryInstance();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            destroyDiaryInstance();
        }


        void createDiaryInstance()
        {
            _diaryInstance = DiaryPrototypeRoot.InstantiateWithTransform();
            _diaryInstance.SetActive(true);
        }
        void destroyDiaryInstance()
        {
            if (_diaryInstance) Destroy(_diaryInstance);
            _diaryInstance = null;
        }
    }
}
