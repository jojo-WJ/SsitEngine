/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：三次hermite样条曲线                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:18:24                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Mathematics
{
    /// <summary>Piecewise three hermite spline curve.</summary>
    public class HermiteCurve
    {
        /// <summary>Key frames of curve.</summary>
        protected List<KeyFrame> frames = new List<KeyFrame>();

        /// <summary>Constructor.</summary>
        public HermiteCurve()
        {
        }

        /// <summary>Constructor.</summary>
        /// <param name="frames">Key frames of curve.</param>
        public HermiteCurve( KeyFrame[] frames )
        {
            this.frames.AddRange(frames);
        }

        /// <summary>Indexer.</summary>
        /// <param name="index">Index of key frame.</param>
        /// <returns>The key frame at index.</returns>
        public KeyFrame this[ int index ] => frames[index];

        /// <summary>Count of key frames.</summary>
        public int KeyFramesCount => frames.Count;

        /// <summary>Evaluate the value of hermite curve at time.</summary>
        /// <param name="t">Time of curve to evaluate value.</param>
        /// <returns>The value of hermite curve at time.</returns>
        public double Evaluate( double t )
        {
            return Evaluate(frames.ToArray(), t);
        }

        /// <summary>Add key frame to curve.</summary>
        /// <param name="time">Time of key frame.</param>
        /// <param name="value">Value of key frame.</param>
        public void AddKeyFrame( double time, double value )
        {
            frames.Add(new KeyFrame(time, value));
        }

        /// <summary>Add key frame to curve.</summary>
        /// <param name="keyFrame">Key frame to add.</param>
        public void AddKeyFrame( KeyFrame keyFrame )
        {
            frames.Add(keyFrame);
        }

        /// <summary>Remove key frame at index.</summary>
        /// <param name="index">Index of key frame.</param>
        public void RemoveKeyFrameAt( int index )
        {
            frames.RemoveAt(index);
        }

        /// <summary>
        ///     Evaluate the value of hermite curve at time on the range from start key frame to end key frame.
        /// </summary>
        /// <param name="start">Start key frame of hermite curve.</param>
        /// <param name="end">End key frame of hermite curve.</param>
        /// <param name="t">Time of curve to evaluate value.</param>
        /// <returns>The value of hermite curve at time on the range from start key frame to end key frame.</returns>
        public static double Evaluate( KeyFrame start, KeyFrame end, double t )
        {
            return Hermite.Evaluate(start.time, end.time, start.value, end.value, start.outTangent, end.inTangent, t);
        }

        /// <summary>Evaluate the value of hermite curve at time.</summary>
        /// <param name="frames">Key frames of hermite curve.</param>
        /// <param name="t"></param>
        /// <returns>The value of hermite curve at time.</returns>
        public static double Evaluate( KeyFrame[] frames, double t )
        {
            if (frames == null || frames.Length == 0)
            {
                return 0.0;
            }
            var num = 0.0;
            if (frames.Length == 1)
            {
                num = frames[0].value;
            }
            else if (t <= frames[0].time)
            {
                num = frames[0].value;
            }
            else if (t >= frames[frames.Length - 1].time)
            {
                num = frames[frames.Length - 1].value;
            }
            else
            {
                for (var index = 0; index < frames.Length; ++index)
                {
                    if (index == frames[index].time)
                    {
                        num = frames[index].value;
                        break;
                    }
                    if (t > frames[index].time && t < frames[index + 1].time)
                    {
                        num = Evaluate(frames[index], frames[index + 1], t);
                        break;
                    }
                }
            }
            return num;
        }
    }
}