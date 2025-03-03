using System;
using SETHD.FantasySnake.Command;
using TMPro;
using UnityEngine;
using VitalRouter;

namespace SETHD.FantasySnake.UI
{
    [Serializable]
    public class UIAsset
    {
        public GameObject GroupImage => groupImage;

        public TextMeshProUGUI NameTMP => nameTMP;

        public TextMeshProUGUI AttackTMP => attackTMP;

        public TextMeshProUGUI DefenseTMP => defenseTMP;

        public TextMeshProUGUI HealthTMP => healthTMP;

        [SerializeField]
        private GameObject groupImage;

        [SerializeField]
        private TextMeshProUGUI nameTMP;
        
        [SerializeField]
        private TextMeshProUGUI attackTMP;
        
        [SerializeField]
        private TextMeshProUGUI defenseTMP;
        
        [SerializeField]
        private TextMeshProUGUI healthTMP;
    }
    
    [Routes]
    public partial class UIController
    {
        private readonly UIAsset uiAsset;

        public UIController(UIAsset uiAsset)
        {
            this.uiAsset = uiAsset;
        }

        [Route]
        private void OnHover(HeroHoverCommand command)
        {
            uiAsset.GroupImage.SetActive(true);
            uiAsset.NameTMP.text = command.Name;
            uiAsset.AttackTMP.text = command.Attack;
            uiAsset.DefenseTMP.text = command.Defense;
            uiAsset.HealthTMP.text = command.Health;
        }

        [Route]
        private void OnCancelHover(HoverCancelHoverCommand command)
        {
            uiAsset.GroupImage.SetActive(false);
        }
    }
}
