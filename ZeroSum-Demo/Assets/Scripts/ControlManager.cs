using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlManager : MonoBehaviour
{

    #region Singleton
    private static ControlManager _instance;
    public static ControlManager Instance { get { return _instance; } }
    #endregion
    //yanda toplar olacak hak yerine geçecek
    //  List<GameObject> Balls = new List<GameObject>();
    GameObject[] Balls;
    #region Ball values
    public Vector3 LaunchPointFinal;

    #endregion

    public float h;

    #region Score Calculation Values

    bool FireInTheHole = false;   //Top yüklendiğinde true olacak zaman sayımı için

    public float BeforeShotTime;  //top cannon'a yüklendikten sonra geçen süreyi ifade ediyor. Puanlama için kullanıalacak

    public float MaxWaitTime = 5f;
    #endregion


    #region UI
    [Header("UI Variables")]
    public TextMeshProUGUI DisplayScoreUI;
    public Transform FinishPanel;
    public Slider IngameSlider;
    public GameObject RestartButton;
    #endregion


    public Transform Target;
    public Transform RoadBallsParent;
    Rigidbody _currentBall;
    public float hMax;
    public float gravity = -10;
    float _time;
    Vector3 _initialVelocity;
    List<Transform> RoadBalls = new List<Transform>();

    bool _isNan = false;

    public Transform CurrentWall;
    List<CubeScript> WallPoint = new List<CubeScript>();
    public GameObject ExplosionEffect;

    public bool IsPlayable = true;

    public List<Transform> ExplosionPoints = new List<Transform>();
    int _score;
    List<GameObject> BallsForShoot = new List<GameObject>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        foreach (Transform item in RoadBallsParent)
        {
            RoadBalls.Add(item);
            item.GetComponent<MeshRenderer>().enabled = false;
        }


    }

    private void Start()
    {
        DOTween.Init();
        //yolları temsil edecek topları ekledik.

        WallPoint = CurrentWall.GetComponentsInChildren<CubeScript>().ToList();


        Balls = GameObject.FindGameObjectsWithTag("Player");

        Restart();
        IngameSlider.maxValue = MaxWaitTime;
    }

    private void Update()
    {
        if (IsPlayable)
        {
            //eğer ki top cannon'a yüklendiyse true
            if (FireInTheHole)
            {
                //slider'ı zmanla azal
                IngameSlider.value -= Time.deltaTime;
                //mevcut sürede atış yapılmazsa patlar ve yok olur.
                if (IngameSlider.value <= 0)
                {

                    ExplosionEffect.SetActive(true);
                    GameOver();
                }
                else
                {
                    if (Input.GetMouseButton(0))
                    {
                        //sürekli bir raycast gönder
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        //çarptığı pozisyona targeti konumlandır, cannon o noktaya baksın ve atış yapılacak yolu hesapla.
                        if (Physics.Raycast(ray, out hit))
                        {
                            Target.position = hit.point;
                            transform.LookAt(Target);
                            CalculatePath();
                        }
                    }
                    //eğer yol düzgün bir şekilde çizilmişse atış yap.
                    else if (Input.GetMouseButtonUp(0) && _isNan == false)
                    {
                        Shoot();
                    }
                }
            }
            //topu cannon'a yükle
            else if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Debug.Log(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        SelectBall(hit.collider.gameObject);
                    }
                }
            }
        }
    }


    #region Shoot Funcs


    public void WallExplosion()
    {
        for (int i = 0; i < ExplosionPoints.Count; i++)
        {
            ExplosionPoints[i].GetChild(0).gameObject.SetActive(true);
            ExplosionPoints[i].GetComponent<MeshRenderer>().enabled = false;
        }
        foreach (Transform item in CurrentWall)
        {
            for (int i = 0; i < ExplosionPoints.Count; i++)
            {
                item.GetComponent<Rigidbody>().AddExplosionForce(8f, ExplosionPoints[i].transform.position, 5f);
            }
        }
        GameOver();
    }
    //atış fonksiyonu
    void Shoot()
    {
        foreach (Transform item in CurrentWall)
        {
            item.GetComponent<Rigidbody>().isKinematic = false;
        }
        ClosePath();
        _currentBall.GetComponent<MeshRenderer>().enabled = true;
        _currentBall.GetComponent<ConstantForce>().force = (Vector3.up * gravity) - Physics.gravity;
        // Physics.gravity = Vector3.up * gravity;
        _currentBall.useGravity = true;
        _currentBall.velocity = _initialVelocity;
        FireInTheHole = false;
    }

    //atış yapılacak yolu hesaplayıp çizer.
    void CalculatePath()
    {
        //t iniş için gerekli olan mesafe
        float displacementY = Target.position.y - _currentBall.position.y;
        // X ve Z deki yer değiştirme
        Vector3 displacementXZ = new Vector3(Target.position.x - _currentBall.position.x, 0, Target.position.z - _currentBall.position.z);
        //t uçuş + t iniş toplam zamanı
        // tuçuş = 2hmax/gravity'nin karekökü
        //tiniş = y'deki yer değiştirme *2 / gravity'nin karekökü
        _time = Mathf.Sqrt(-2 * hMax / gravity) + Mathf.Sqrt(2 * (displacementY - hMax) / gravity);

        //Vy'nin karesi = -2 * gravity * hmax
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * hMax);
        //Vxz' XZ'deki yerdeğiştirme / zzaman
        Vector3 velocityXZ = displacementXZ / _time;

        _initialVelocity = velocityXZ + velocityY * -Mathf.Sign(gravity);
        Vector3 previousDrawPoint = _currentBall.position;
        //yyolu göstermek için gerekli olan sayi
        int resolution = 10;
        if (!float.IsNaN(_time))
        {
            Target.GetComponent<MeshRenderer>().enabled = true;
            _isNan = false;

            for (int i = 1; i < resolution; i++)
            {
                float simulationTime = i / (float)resolution * _time;

                Vector3 displacement = _initialVelocity * simulationTime + Vector3.up * gravity * simulationTime * simulationTime / 2f;
                Vector3 drawPoint = _currentBall.position + displacement;
                RoadBalls[i - 1].GetComponent<MeshRenderer>().enabled = true;
                RoadBalls[i - 1].localPosition = drawPoint;
                previousDrawPoint = drawPoint;
            }
        }
        else
        {
            _isNan = true;
            ClosePath();
        }

    }

    //çizdiği yolu kapatır.
    void ClosePath()
    {
        Target.GetComponent<MeshRenderer>().enabled = false;
        Target.localPosition = Vector3.zero;
        for (int i = 0; i < RoadBalls.Count; i++)
        {
            RoadBalls[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    #endregion


    #region select Funcs
    // o topa bastığında top otomatikman topun içine girecek ( animasyon)
    void SelectBall(GameObject selectedBall)
    {
        _currentBall = selectedBall.GetComponent<Rigidbody>();
        _currentBall.isKinematic = false;
        BallsForShoot.Remove(selectedBall);
        _currentBall.DOJump(LaunchPointFinal, 1f, 1, 0.5f).OnComplete(() => PrepareShoot());
    }

    void PrepareShoot()
    {
        IngameSlider.value = MaxWaitTime;
        FireInTheHole = true;
        _currentBall.GetComponent<MeshRenderer>().enabled = false;
    }

    #endregion


    #region Finish Funcs
    public void CheckBallsCount()
    {
        if (BallsForShoot.Count == 0)
        {
            GameOver();
        }
    }

    void CalculateScore()
    {
        _score = 0;
        float extraTime = IngameSlider.value;
        if (extraTime != 0)
        {
            _score += CurrentWall.childCount / 5;
            _score += BallsForShoot.Count * 5;
            _score += (int)extraTime * 2;
        }
    }

    void ResetWall()
    {
        foreach (CubeScript item in WallPoint)
        {
            item.ResetCubes();
        }

        foreach (Transform item in ExplosionPoints)
        {
            item.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    //OYunu yenileyen veya başlangıçta çalışan fonksiyon
    public void Restart()
    {
        ResetWall();
        _score = 0;
        ExplosionEffect.SetActive(false);
        Target.GetComponent<MeshRenderer>().enabled = false;

        IsPlayable = true;
        FinishPanel.gameObject.SetActive(false);
        IngameSlider.maxValue = MaxWaitTime;
        IngameSlider.value = MaxWaitTime;
        transform.localRotation = Quaternion.identity;
        BallsForShoot = Balls.ToList();
        foreach (GameObject item in BallsForShoot)
        {
            item.GetComponent<BallScript>().ResetBallsPos();
        }
    }

    //Oyun bittiğinde çalışan fonksiyon
    void GameOver()
    {
        ClosePath();
        CalculateScore();
        FireInTheHole = false;
        IsPlayable = false;
        FinishPanel.gameObject.SetActive(true);
        StartCoroutine(DisplayScore(_score));
    }

    //ekrana score'u yazmasını sağlayan fonksiyon
    IEnumerator DisplayScore(int Finalscore)
    {
        int displayScore = 0;
        while (true)
        {
            if (displayScore < Finalscore)
            {
                displayScore++;
                DisplayScoreUI.text = displayScore.ToString();
            }
            else if (displayScore == Finalscore)
            {
                RestartButton.SetActive(true);
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    #endregion

}
