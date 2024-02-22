using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

// RollerAgent
public class CartPoleAgent : Agent
{
    public GameObject pole; 
    Rigidbody poleRB;
    Rigidbody cartRB;
    EnvironmentParameters m_ResetParams;


    //初期値
    public override void Initialize()
    {
        //学習の初期化
        this.poleRB = pole.GetComponent<Rigidbody>();
        this.cartRB = GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }

    //センサーにデータを送る
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pole.transform.localPosition.z);
        sensor.AddObservation(cartRB.velocity.z);
        sensor.AddObservation(pole.transform.localRotation.eulerAngles.x);
        sensor.AddObservation(poleRB.angularVelocity.x);
    }
    //各ステップでの行動
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //カートに力を加える
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f) * 200f;
        cartRB.AddForce(controlSignal);

        //カートの位置、ポールの角度と角速度
        float cart_z = this.transform.localPosition.z;
        float angle_x = pole.transform.localRotation.eulerAngles.x;

        //angle_zを-180~180に変換
        if(180f < angle_x && angle_x < 360f)
        {
            angle_x = angle_x - 360f;
        }

        //カートが+-45度いないなら報酬+0.1 それ以外は -1
        if((-180f < angle_x && angle_x < -45f) || (45f < angle_x && angle_x < 180f))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        else{
            SetReward(0.1f);
        }
        //カートの位置が-10~10の範囲を超えたら報酬-1
        if(cart_z < -10f || 10f < cart_z)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    //ステップ開始の初期条件を決定
    public override void OnEpisodeBegin()
    {
        //エージェントの状態をリセット
        transform.localPosition = new Vector3(0f, 0f, 0f);
        pole.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        pole.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        poleRB.angularVelocity = new Vector3(0f, 0f, 0f);
        poleRB.velocity = new Vector3(0f, 0f, 0f);
        //ポールにランダムな傾きを与える
        poleRB.angularVelocity = new Vector3(Random.Range(-0.1f, 0.1f), 0f, 0f);
        SetResetParameters();
    }

    //キーボードから操作する場合
    public override void Heuristic(in ActionBuffers actionBuffers)
    {
        var continuousActionsOut = actionBuffers.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }

    //ポールの条件をリセット
    public void SetPole()
    {
        poleRB.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
        pole.transform.localScale = new Vector3(0.4f, 2f, 0.4f);
    }

    //パラメータをリセットする関数
    public void SetResetParameters()
    {
        SetPole();
    }
}