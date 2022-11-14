using UnityEngine;
using Core;

public class EntryPoint : MonoBehaviour
{
    // MonoBehaviour
    [Header("Links")]
    [SerializeField] private LifeUi _lifeUi;
    [SerializeField] private RestartUi _restartUI;
    [SerializeField] private CameraMover _camera;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _сellViewContainer;
    
    // Settings
    [Header("Settings")]
    [SerializeField] private bool _isGameFieldSquare;
    [Range(0, 5)] [SerializeField] private int _startValueRow = 5;
    [Range(1, 10)] [SerializeField] private int _maxValueRow = 10;
    [Range(0, 5)] [SerializeField] private int _startValueColumn = 5;
    [Range(1, 10)] [SerializeField] private int _maxValueColumn = 10;
    [Range(2, 10)] [SerializeField] private float _distanceBetweenCells;
    [Range(0, 3)] [SerializeField] private int _lifeCount;
    // Core
    private WayBuilder _wayBuilder;
    private CellViewController _cellViewController; 
    private SceneLoader _sceneLoader;
    private GameplayController _gameplayController;
    private CellHighlighter _cellHighlighter;
    
    private void Awake()
    {
        _wayBuilder = new WayBuilder(_isGameFieldSquare, _startValueRow, _maxValueRow, 
                                    _startValueColumn, _maxValueColumn);
        _cellViewController = new CellViewController(_wayBuilder, _cellViewPrefab, _camera, 
                                                    _сellViewContainer,_distanceBetweenCells);
        _sceneLoader = new SceneLoader();
        _cellHighlighter = new CellHighlighter();
        _gameplayController = new GameplayController(_wayBuilder, _cellViewController, _cellHighlighter, 
                                                    _sceneLoader, _lifeCount);
    }

    private void Start()
    {
        _wayBuilder.CreateConfig();
        _cellViewController.CreateCellsView();
        _lifeUi.SetCountLife(_lifeCount);
        _gameplayController.UpdateLifeCount += _lifeUi.SetCountLife;
        _restartUI.RestartClick += _gameplayController.CallDefeat;
        _gameplayController.Initialize();
    }
}
