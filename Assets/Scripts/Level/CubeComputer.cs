using SSpot.UI;
using SSpot.Utilities;
using UnityEngine;

namespace SSpot.Level
{
    public class CubeComputer : MonoBehaviour
    {
        [Header("Cells")]
        [SerializeField] private AttachingCube[] cubeCells = {};
        [SerializeField] private LoopController[] loopCells = {};

        [Header("Buttons")]
        [SerializeField] private PointerButton runButton;
        [SerializeField] private PointerButton resetButton;
        [SerializeField] private PointerButton clearButton;
        
        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip errorSound;
        
        [Header("Materials")]
        [SerializeField] private MeshRenderer terminalRenderer;
        [SerializeField] private Material successTerminalMaterial;
        [SerializeField] private Material errorTerminalMaterial;
        [SerializeField] private float materialResetDelay = 5f;

        private Material _originalTerminalMaterial;

        private void Awake() => _originalTerminalMaterial = terminalRenderer.materials[1];

        private void OnEnable()
        {
            runButton.OnPointerClickEvent.AddListener(OnRunButtonPressed);
            resetButton.OnPointerClickEvent.AddListener(OnResetButtonPressed);
            clearButton.OnPointerClickEvent.AddListener(OnClearPressed);
            
            LevelManager.Instance.OnSuccess.AddListener(OnSuccess);
            LevelManager.Instance.OnError.AddListener(OnError);
            LevelManager.Instance.OnReset.AddListener(OnReset);
        }
        
        private void OnDisable()
        {
            runButton.OnPointerClickEvent.RemoveListener(OnRunButtonPressed);
            resetButton.OnPointerClickEvent.RemoveListener(OnResetButtonPressed);
            clearButton.OnPointerClickEvent.RemoveListener(OnClearPressed);
            
            if (LevelManager.Instance)
            {
                LevelManager.Instance.OnSuccess.RemoveListener(OnSuccess);
                LevelManager.Instance.OnError.RemoveListener(OnError);
                LevelManager.Instance.OnReset.RemoveListener(OnReset);
            }
        }

        public void ClearCells()
        {
            foreach (var cube in cubeCells)
                if (cube.CurrentCube != null)
                    cube.ClearCellRPC();
        }
        
        #region Button Callbacks
        
        private void OnRunButtonPressed() => LevelManager.Instance.Run(cubeCells, loopCells);
        
        private void OnResetButtonPressed() => LevelManager.Instance.Reset();
        
        private void OnClearPressed()
        {
            OnResetButtonPressed();
            ClearCells();
        }
        
        #endregion

        #region  LevelManager Callbacks

        private Coroutine _materialResetCoroutine;
        
        private void SetMaterial(Material material, bool reset)
        {
            if (_materialResetCoroutine != null)
                StopCoroutine(_materialResetCoroutine);
            
            var mats = terminalRenderer.materials;
            mats[1] = material;
            terminalRenderer.materials = mats;

            if (reset)
                _materialResetCoroutine = StartCoroutine(CoroutineUtilities.WaitThen(materialResetDelay, OnReset));
        }
        
        private void OnSuccess()
        {
            SetMaterial(successTerminalMaterial, reset: true);
            audioSource.PlayOneShot(successSound);
        }

        private void OnError()
        {
            SetMaterial(errorTerminalMaterial, reset: true);
            audioSource.PlayOneShot(errorSound);
        }
        
        private void OnReset() => SetMaterial(_originalTerminalMaterial, reset: false); 
        
        #endregion

        public void AddPlayerHand(int playerViewId)
        {
            foreach(var attachingCube in cubeCells)
            {
                attachingCube.AddPlayerHand(playerViewId);
            }
        }
    }
}