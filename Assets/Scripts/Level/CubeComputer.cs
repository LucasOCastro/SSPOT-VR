using System;
using System.Collections.Generic;
using SSpot.Ambient.ComputerCode;
using SSpot.UI;
using SSpot.Utilities;
using UnityEngine;

namespace SSpot.Level
{
    public class CubeComputer : MonoBehaviour
    {
        [Header("Cells")]
        [SerializeField] private Transform cellsParent;

        [Header("Buttons")]
        [SerializeField] private PointerButton runButton;
        [SerializeField] private PointerButton resetButton;
        [SerializeField] private PointerButton clearButton;
        
        [Header("Sounds")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip errorSound;
        
        [Header("Materials")]
        [SerializeField] private MeshRenderer terminalRenderer;
        [SerializeField] private Material successTerminalMaterial;
        [SerializeField] private Material errorTerminalMaterial;
        [SerializeField] private float materialResetDelay = 5f;

        private CodingCell[] _cells = Array.Empty<CodingCell>();
        public IReadOnlyList<CodingCell> Cells => _cells;

        //Consider having an index in each cell, and have AttachingCube and LoopController keep a reference to parent cell
        public int IndexOf(AttachingCube cube) => _cells.FindIndex(cell => cell.AttachingCube == cube);
        public int IndexOf(LoopController loop) => _cells.FindIndex(cell => cell.LoopController == loop);

        private Material _originalTerminalMaterial;

        private void Awake()
        {
            _originalTerminalMaterial = terminalRenderer.materials[1];
            
            _cells = transform.GetComponentsInChildren<CodingCell>();
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].Init(i, this);
            }
        }

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
            foreach (var cell in _cells)
                cell.Clear();
        }
        
        #region Button Callbacks
        
        private void OnRunButtonPressed()
        {
            LevelManager.Instance.Run(Cells);
        }
        
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
    }
}