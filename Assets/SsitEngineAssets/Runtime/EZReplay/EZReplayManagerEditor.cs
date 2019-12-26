#if UNITY_EDITOR
using Framework.Utility;
using UnityEngine;

namespace SsitEngine.EzReplay
{
    public partial class EZReplayManager
    {
        // these values determine what GUIs will be seen by the user:
        public bool showHintsForImportantOptions;

        //default: true
        public bool useDarkStripesInReplay;

        public bool useRecordingGUI;

        //default: true
        public bool useReplayGUI;

        //private float tt =0;

        //create an empty prefab at predefined location
        /*protected UnityEngine.Object CreateEmptyEZRPrefab(string filepath)
        {
            return PrefabUtility.CreateEmptyPrefab("Assets/Resources/" + filepath + ".prefab");
        }*/

        private void OnGUI()
        {
            var style = GUI.skin.GetStyle("box");
            style.fontStyle = FontStyle.Bold;

            if (showPrecachingMandatoryMessage)
                GUI.Box(new Rect(Screen.width / 2 - 650 / 2, Screen.height / 2 - 40 / 2, 650, 40),
                    "To use this functionality, please enable precaching of game objects (in EZReplayManager prefab)",
                    style);
            if (CurrentMode == ViewMode.LIVE && useRecordingGUI)
            {
                Rect r0;

                if (CurrentAction == ActionMode.RECORD || maxPosition > 0)
                    r0 = new Rect(10, 20, 150, 200);
                else
                    r0 = new Rect(10, 20, 150, 170);

                //GUI.Box( r0, EZRicon );

                var r1 = new Rect(20, 160, 130, 20);
                var r2 = new Rect(20, 190, 130, 20);


                if (CurrentAction == ActionMode.STOPPED || CurrentAction == ActionMode.READY)
                {
                    useDarkStripesInReplay = false;
                    //tt =0;

                    if (OnEZRecord == null)
                        GUI.color = Color.white;
                    else
                        GUI.color = Color.green;
                    if (GUI.Button(r1, "录制"))
                    {
                        CurrentAction = ActionMode.READY;
                        Instance.OnEZRecord = () =>
                        {
                            CurrentAction = ActionMode.STOPPED;
                            Record();
                            Instance.OnEZRecord = null;
                        };
                    }
                    GUI.color = Color.white;
                }
                else if (CurrentAction == ActionMode.RECORD)
                {
                    if (GUI.Button(r1, "暂停")) Stop();
                }

                if (maxPosition > 0)
                    if (GUI.Button(r2, "回放"))
                    {
                        Stop();
                        Play(0, false, false, false);
                    }
                if (GUI.Button(new Rect(20, 280, 130, 20), "Save replay"))
                    Instance.SaveToFile("example1.ezr");
                else if (GUI.Button(new Rect(20, 310, 130, 20), "Load replay")) Instance.LoadFromFile("example1.ezr");
            }
            else if (CurrentMode == ViewMode.REPLAY)
            {
                useDarkStripesInReplay = true;


                if (useReplayGUI)
                {
                    //<!-- SPEED SLIDER
                    var Stopped = false;

                    if (speedSliderValue <= minSpeedSliderValue)
                        Stopped = true;

                    speedSliderValue = (int) GUI.HorizontalSlider(new Rect(10, 60, 120, 70), speedSliderValue,
                        minSpeedSliderValue, maxSpeedSliderValue);
                    //setReplaySpeed: bad function name
                    var speedIndicator = SetReplaySpeed(speedSliderValue);

                    if (Stopped && playingInterval > 0.0f) Stopped = false;

                    GUI.Box(new Rect(10, 10, 130, 45),
                        "Replay speed:\n" +
                        speedIndicator); //+ "\n, interval: "+playingInterval+", step: "+recorderPositionStep);

                    // SPEED SLIDER //--> 

                    //<!-- TIME SLIDER 
                    var recorderPositionTemp =
                        (int) GUI.HorizontalSlider(new Rect(150, 60, 240, 10), recorderPosition, 0, maxPosition);

                    if (recorderPositionTemp != recorderPosition)
                    {
                        Pause();
                        recorderPosition = recorderPositionTemp;
                        Instance.ExecRecorderAction(true);
                        Pause();
                    }

                    var percentage = Mathf.Round(recorderPosition / (float) maxPosition * 100.0f);
                    GUI.Box(new Rect(150, 10, 240, 45), "Replay position: " + percentage + "%");
                    // TIME SLIDER //--> 

                    //<!-- POSITION MANIPULATION TOOLS 
                    if (GUI.Button(new Rect(158, 72, 40, 23), "播放")) Play(speedSliderValue, true, false, false);

                    if (GUI.Button(new Rect(203, 72, 40, 23), "倒播")) Play(speedSliderValue, true, true, false);

                    if (GUI.Button(new Rect(248, 72, 40, 23), "暂停")) Pause();

                    if (GUI.Button(new Rect(293, 72, 40, 23), "停止")) Stop();

                    if (GUI.Button(new Rect(338, 72, 40, 23), "关闭")) SwitchModeTo(ViewMode.LIVE);
                    // POSITION MANIPULATION TOOLS //-->


                    if (GUI.Button(new Rect(20, 280, 130, 20), "Save replay"))
                        Instance.SaveToFile("example1.ezr");
                    else if (GUI.Button(new Rect(20, 310, 130, 20), "Load replay"))
                        Instance.LoadFromFile("example1.ezr");

                    if (useDarkStripesInReplay)
                    {
                        GUI.Box(new Rect(0, 0, Screen.width, 100), "");

                        //GUILayout.BeginArea(new Rect( 0, Screen.height - 100, Screen.width, Screen.height - 100 ), "" );
                        //Debug.Log( "expression" + recordBeginTime + " end " + recordEndTime );
                        var dt = Utilitys.ConvertLongToDateTime(RecordEndTime) -
                                 Utilitys.ConvertLongToDateTime(RecordBeginTime);
                        //this.tt += Time.deltaTime;
                        //Debug.Log("expression"+tt);
                        var timer = string.Format("{0}:{1}:{2}", dt.Hours, dt.Minutes, dt.Seconds);
                        GUI.Label(new Rect(0, 0, Screen.width, 100), timer,
                            new GUIStyle {alignment = TextAnchor.MiddleCenter, fontSize = 60});
                    }
                }
            }
        }
    }
}

#endif