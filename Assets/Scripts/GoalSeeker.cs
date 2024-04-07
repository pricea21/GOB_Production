using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalSeeker : MonoBehaviour
{
    Goal[] mGoals;
    Action[] mActions;
    Action mChangeOverTime;
    const float TICK_LENGTH = 5.0f;

    public Text GoalText;
    public Text ToDoText;
    public Text ClockText;

    public AudioClip PlayAudio;
    public AudioClip SleepAudio;
    public AudioClip GetReadyAudio;

    private AudioSource mAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        mAudioSource = GetComponent<AudioSource>();

        // my inital motives/goals
        mGoals = new Goal[3];
        mGoals[0] = new Goal("Get Ready", 4);
        mGoals[1] = new Goal("Sleep", 3);
        mGoals[2] = new Goal("Play", 3);

        // the actions I know how to do
        mActions = new Action[6];
        mActions[0] = new Action("get ready for school later");
        mActions[0].targetGoals.Add(new Goal("Get Ready", -3f));
        mActions[0].targetGoals.Add(new Goal("Sleep", +2f));
        mActions[0].targetGoals.Add(new Goal("Play", +1f));

        mActions[1] = new Action("get ready now");
        mActions[1].targetGoals.Add(new Goal("Get Ready", -2f));
        mActions[1].targetGoals.Add(new Goal("Sleep", -1f));
        mActions[1].targetGoals.Add(new Goal("Play", +1f));

        mActions[2] = new Action("sleep in the bed");
        mActions[2].targetGoals.Add(new Goal("Get Ready", +2f));
        mActions[2].targetGoals.Add(new Goal("Sleep", -4f));
        mActions[2].targetGoals.Add(new Goal("Play", +2f));

        mActions[3] = new Action("sleep in moms room");
        mActions[3].targetGoals.Add(new Goal("Get Ready", +1f));
        mActions[3].targetGoals.Add(new Goal("Sleep", -2f));
        mActions[3].targetGoals.Add(new Goal("Play", +1f));

        mActions[4] = new Action("play with blocks");
        mActions[4].targetGoals.Add(new Goal("Get Ready", -1f));
        mActions[4].targetGoals.Add(new Goal("Sleep", -2f));
        mActions[4].targetGoals.Add(new Goal("Play", +3f));

        mActions[5] = new Action("play outside");
        mActions[5].targetGoals.Add(new Goal("Get Ready", 0f));
        mActions[5].targetGoals.Add(new Goal("Sleep", 0f));
        mActions[5].targetGoals.Add(new Goal("Play", -4f));

        // the rate my goals change just as a result of time passing
        mChangeOverTime = new Action("tick");
        mChangeOverTime.targetGoals.Add(new Goal("Get Ready", +4f));
        mChangeOverTime.targetGoals.Add(new Goal("Sleep", +1f));
        mChangeOverTime.targetGoals.Add(new Goal("Play", +2f));

        ClockText.text = "Starting clock. One hour will pass every " + TICK_LENGTH + " seconds.";
        InvokeRepeating("Tick", 0f, TICK_LENGTH);

        Debug.Log("Hit E to do something.");
    }

    void Tick()
    {
        // apply change over time
        foreach (Goal goal in mGoals)
        {
            goal.value += mChangeOverTime.GetGoalChange(goal);
            //Debug.Log(mChangeOverTime.GetGoalChange(goal));
            goal.value = Mathf.Max(goal.value, 0);
        }

        // print results
        PrintGoals();
    }

    void PrintGoals()
    {
        string goalString = "";
        foreach (Goal goal in mGoals)
        {
            goalString += goal.name + ": " + goal.value + "; ";
        }
        goalString += "Discontentment: " + CurrentDiscontentment();
        GoalText.text = goalString;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            Action bestThingToDo = ChooseAction(mActions, mGoals);
            //Debug.Log("-- BEST ACTION --");

            if (bestThingToDo.name == "get ready now")
            {
                mAudioSource.PlayOneShot(GetReadyAudio);
            }

            if (bestThingToDo.name == "sleep in the bed")
            {
                mAudioSource.PlayOneShot(SleepAudio);
            }

            if (bestThingToDo.name == "play outside")
            {
                mAudioSource.PlayOneShot(PlayAudio);
            }

            ToDoText.text = "I think I will " + bestThingToDo.name;

            // do the thing
            foreach (Goal goal in mGoals)
            {
                goal.value += bestThingToDo.GetGoalChange(goal);
                goal.value = Mathf.Max(goal.value, 0);
            }

            //Debug.Log("-- NEW GOALS --");
            PrintGoals();
        }
    }

    Action ChooseAction(Action[] actions, Goal[] goals)
    {

        // find the action leading to the lowest discontentment
        Action bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (Action action in actions)
        {
            float thisValue = Discontentment(action, goals);
            //Debug.Log("Maybe I should " + action.name + ". Resulting discontentment = " + thisValue);
            if (thisValue < bestValue)
            {
                bestValue = thisValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    float Discontentment(Action action, Goal[] goals)
    {
        // keep a running total
        float discontentment = 0f;

        // loop through each goal
        foreach (Goal goal in goals)
        {
            // calculate the new value after the action
            float newValue = goal.value + action.GetGoalChange(goal);
            newValue = Mathf.Max(newValue, 0);

            // get the discontentment of this value
            discontentment += goal.GetDiscontentment(newValue);
        }

        return discontentment;
    }

    float CurrentDiscontentment()
    {
        float total = 0f;
        foreach (Goal goal in mGoals)
        {
            total += (goal.value * goal.value);
        }
        return total;
    }
}
