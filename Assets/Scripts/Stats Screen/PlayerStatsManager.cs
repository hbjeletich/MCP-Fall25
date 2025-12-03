using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    public class LimbStats
    {
        public int totalPrompts;
        public int totalHits;
        public int totalMisses;
        public float totalAccuracy;
    }

    public class QTEStats
    {
        public int totalQTEs;
        public int successfulPresses;
        public int missedPresses;
        public float totalDeviation;
    }

    public class HidingStats
    {
        public int totalAttempts;
        public int successfulAttempts;
    }

    public LimbStats[] limbStats = new LimbStats[4];
    public QTEStats[] qteStats = new QTEStats[5];
    public HidingStats[] hidingStats = new HidingStats[5];

    private LimbStats headLimbStat = new LimbStats();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < 4; i++)
            limbStats[i] = new LimbStats();

        for (int i = 0; i < 5; i++)
            qteStats[i] = new QTEStats();

        for (int i = 0; i < 5; i++)
            hidingStats[i] = new HidingStats();

    }

    public void RecordRunningPrompt(int limbIndex)
    {
        limbStats[limbIndex].totalPrompts++;
    }

    public void RecordRunningHit(int limbIndex, float accuracy)
    {
        limbStats[limbIndex].totalHits++;
        limbStats[limbIndex].totalAccuracy += accuracy;
    }

    public void RecordRunningMiss(int limbIndex)
    {
        limbStats[limbIndex].totalMisses++;
    }

    public void RecordQTEPress(int playerIndex, float deviation)
    {
        qteStats[playerIndex].totalQTEs++;
        qteStats[playerIndex].successfulPresses++;
        qteStats[playerIndex].totalDeviation += deviation;
    }

    public void RecordQTEMiss(int playerIndex)
    {
        qteStats[playerIndex].totalQTEs++;
        qteStats[playerIndex].missedPresses++;
    }

    public void RecordHidingSuccess()
    {
        for (int i = 0; i < 5; i++)
        {
            hidingStats[i].totalAttempts++;
            hidingStats[i].successfulAttempts++;
        }
    }

    public void RecordHidingFail()
    {
        for (int i = 0; i < 5; i++)
        {
            hidingStats[i].totalAttempts++;
        }
    }

    public float GetFinalPlayerAccuracy(int playerIndex)
    {
        float runningAcc = 1f;
        float qteAcc = 1f;
        float hidingAcc = 1f;

        // running
        int runningHits = 0;
        int runningPrompts = 0;
        float runningTotalAcc = 0f;

        for (int i = 0; i < limbStats.Length; i++)
        {
            runningHits += limbStats[i].totalHits;
            runningPrompts += limbStats[i].totalPrompts;
            runningTotalAcc += limbStats[i].totalAccuracy;

            // head stats get average of all limbs (as producer)
            runningHits += headLimbStat.totalHits;
            runningPrompts += headLimbStat.totalPrompts;
            runningTotalAcc += headLimbStat.totalAccuracy;
        }

        if (runningPrompts > 0)
            runningAcc = runningTotalAcc / runningPrompts;


        // QTE
        int qtePresses = qteStats[playerIndex].successfulPresses;
        int qteTotal = qteStats[playerIndex].totalQTEs;

        if (qteTotal > 0)
        {
            float avgDeviation = qteStats[playerIndex].totalDeviation / qtePresses;
            float syncScore = Mathf.Clamp01(1f - (avgDeviation / 0.3f)); // syncWindow
            qteAcc = syncScore;
        }

        // hiding
        int hideAttempts = hidingStats[playerIndex].totalAttempts;
        int hideSuccesses = hidingStats[playerIndex].successfulAttempts;

        if (hideAttempts > 0)
            hidingAcc = (float)hideSuccesses / hideAttempts;

        // average
        float finalScore = (runningAcc + qteAcc + hidingAcc) / 3f;
        //return Mathf.Clamp01(finalScore); // ensure between 0 and 1

        return Mathf.RoundToInt(finalScore * 100f); // percentage
    }


}
