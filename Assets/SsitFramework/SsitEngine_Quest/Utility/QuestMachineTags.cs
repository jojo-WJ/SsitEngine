using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 特定于机器的字符串常量（标记）和实用方法将标记替换为其运行时值
    /// </summary>
    public static class QuestMachineTags
    {
        #region Constants

        /// <summary>
        /// 所有标记都以一个左大括号开始
        /// 
        /// Examples:
        /// - {QUESTGIVER}: Name of quest's giver.
        /// - {Hello}: Looks up "Hello" in the default text table.
        /// - {#numOrcsKilled}: Counter value.
        /// </summary>
        public const string TagPrefix = @"{";

        /// <summary>
        /// All counter value tags start with an open brace and hash sign.
        /// Format can be {#counter} or {#questID:counter}.
        /// 
        /// Examples:
        /// - {#numOrcsKilled}
        /// - {#farming:numApplesPicked}
        /// </summary>
        public const string CounterValueTagPrefix = @"{#";

        /// <summary>
        /// All counter min value tags start with an open brace and less-than sign.
        /// </summary>
        public const string CounterMinValueTagPrefix = @"{<#";

        /// <summary>
        /// All counter max value tags start with an open brace and greater-than sign.
        /// </summary>
        public const string CounterMaxValueTagPrefix = @"{>#";

        /// <summary>
        /// Tag to show a counter value as HH:MM:SS time.
        /// Format can be {:counter} or {:questID:counter}.
        /// 
        /// Examples:
        /// - {:secondsLeft}
        /// </summary>
        public const string CounterTimeValueTagPrefix = @"{:";

        /// <summary>
        /// Separator character between quest name and counter name.
        /// </summary>
        public const string CounterTagQuestNameSeparator = @":";

        /// <summary>
        /// The display name of the quest's giver.
        /// </summary>
        public const string QUESTGIVER = @"{QUESTGIVER}";

        /// <summary>
        /// The ID of the quest's giver.
        /// </summary>
        public const string QUESTGIVERID = @"{QUESTGIVERID}";

        /// <summary>
        /// The display name of the quester.
        /// </summary>
        public const string QUESTER = @"{QUESTER}";

        /// <summary>
        /// The ID of the quest's quester.
        /// </summary>
        public const string QUESTERID = @"{QUESTERID}";

        public const string SINGLESPLIT = "|";
        public const string MULITYSPLIT = ":";

        // Generator tags:
        public const string DOMAIN = @"{DOMAIN}";
        public const string ACTION = @"{ACTION}";
        public const string TARGETDESCRIPTOR = @"{TARGETDESCRIPTOR}";
        public const string TARGET = @"{TARGET}";
        public const string TARGETS = @"{TARGETS}";
        public const string COUNTERGOAL = @"{COUNTERGOAL}";
        public const string REWARD = @"{REWARD}";

        private enum CounterTagType
        {
            Current,
            Min,
            Max,
            AsTime
        }

        #endregion

        #region Utility Methods

        private static bool ContainsAnyTag( string s )
        {
            return s.Contains(TagPrefix);
        }

        private static bool IsDynamicTag( string s )
        {
            return !string.IsNullOrEmpty(s) &&
                   (s.StartsWith(CounterValueTagPrefix) ||
                    s.StartsWith(CounterMinValueTagPrefix) ||
                    s.StartsWith(CounterMaxValueTagPrefix) ||
                    s.StartsWith(CounterTimeValueTagPrefix));
        }

        private static bool IsIDTag( string s )
        {
            return !string.IsNullOrEmpty(s) && (s.Equals(QUESTERID) || s.Equals(QUESTER) || s.Equals(QUESTGIVERID) ||
                                                s.Equals(QUESTGIVER) ||
                                                s.Equals(DOMAIN) || s.Equals(ACTION) || s.Equals(TARGETDESCRIPTOR) ||
                                                s.Equals(TARGET) || s.Equals(TARGETS) ||
                                                s.Equals(COUNTERGOAL) || s.Equals(REWARD));
        }

        public static string GetIDBySpecifier( QuestMessageParticipant specifier, string id )
        {
            switch (specifier)
            {
                case QuestMessageParticipant.Any:
                    return string.Empty;
                case QuestMessageParticipant.Quester:
                    return QUESTERID;
                case QuestMessageParticipant.QuestGiver:
                    return QUESTGIVERID;
                default:
                    return id;
            }
        }

        #endregion

        #region Tag Dictionary Management

        /// <summary>
        /// Adds any tags in a string to a dictionary.
        /// </summary>
        /// <param name="staticTags">Dictionary.</param>
        /// <param name="stringField">string to scan for tags.</param>
        public static void AddTagsToDictionary( TagDictionary staticTags, string s )
        {
            if (!ContainsAnyTag(s) || staticTags == null)
            {
                return;
            }
            foreach (Match match in Regex.Matches(s, @"\{[^\}]+\}"))
            {
                var tag = match.Value;
                if (IsDynamicTag(tag))
                {
                    continue;
                }
                if (staticTags.ContainsTag(tag))
                {
                    continue;
                }
                staticTags.SetTag(tag, string.Empty);
            }
        }

        /// <summary>
        /// Associates tags in a tag dictionary with values from a primary text table 
        /// (i.e., quest giver's text table), or failing that QuestMachine's default text 
        /// table, or failing that the tag name itself. Leaves ID tags such as {QUESTERID}
        /// untouched.
        /// </summary>
        /// <param name="tagDictionary">The tag dictionary containing tags that need values assigned.</param>
        /// <param name="textTable">The primary text table from which to look up values.</param>
        public static void AddTagValuesToDictionary( TagDictionary tagDictionary )
        {
            if (tagDictionary == null)
            {
                return;
            }
            var newDict = new Dictionary<string, string>();
            foreach (var kvp in tagDictionary.dict)
            {
                var tag = kvp.Key;
                if (string.IsNullOrEmpty(tag) || tag.Length <= 2)
                {
                    continue;
                }
                if (IsIDTag(tag))
                {
                    // Leave ID tags untouched:
                    newDict.Add(tag, kvp.Value);
                    continue;
                }
                var fieldName = tag.Substring(1, tag.Length - 2).Trim();

                // Otherwise use the tag's text:
                newDict.Add(tag, fieldName);
            }
            tagDictionary.dict = newDict;
        }

        /// <summary>
        /// If the string has pipe characters, splits values on pipes and returns a random value.
        /// Otherwise returns the entire string.
        /// </summary>
        private static string PrepareFieldText( string s )
        {
            if (string.IsNullOrEmpty(s) || !s.Contains("|"))
            {
                return s;
            }
            var values = s.Split('|');
            return values[Random.Range(0, values.Length)];
        }

        #endregion

        #region Replace Tags

        private static Quest CurrentQuest { get; set; }
        private static TagDictionary QuestTagDictionary { get; set; }
        private static TagDictionary NodeTagDictionary { get; set; }

        /// <summary>
        /// Replaces the tags in a string."{#}{:}"
        /// </summary>
        /// <returns>A string in which the tags have been replaced with their current values.</returns>
        public static string ReplaceTags( string s, Quest quest )
        {
            if (!ContainsAnyTag(s))
            {
                return s;
            }
            CurrentQuest = quest;
            QuestTagDictionary = quest != null ? quest.TagDictionary : null;
            NodeTagDictionary = GetActiveNodeTagDictionary(quest);
            var regex = new Regex(@"\{[^\}]+\}");
            return regex.Replace(s, ReplaceTag);
        }

        private static TagDictionary GetActiveNodeTagDictionary( Quest quest )
        {
            // Return the latest active node's tag dictionary:
            if (quest == null || quest.GetState() != QuestState.Active)
            {
                return null;
            }
            TagDictionary dict = null;
            for (var i = 0; i < quest.NodeList.Count; i++)
            {
                var node = quest.NodeList[i];
                if (node == null)
                {
                    continue;
                }
                if (node.GetState() == QuestNodeState.Active)
                {
                    dict = node.TagDictionary;
                }
            }
            return dict;
        }

        private static string ReplaceTag( Match m )
        {
            var tag = m.ToString();

            if (tag.StartsWith(CounterValueTagPrefix))
            {
                // Replace {#counter} tags:
                return ReplaceCounterTag(tag, CounterTagType.Current);
            }
            if (tag.StartsWith(CounterMinValueTagPrefix))
            {
                // Replace counter min value tags:
                return ReplaceCounterTag(tag, CounterTagType.Min);
            }
            if (tag.StartsWith(CounterMaxValueTagPrefix))
            {
                // Replace counter max value tags:
                return ReplaceCounterTag(tag, CounterTagType.Max);
            }
            if (tag.StartsWith(CounterTimeValueTagPrefix))
            {
                // Replace {:counter} tags:
                return ReplaceCounterTag(tag, CounterTagType.AsTime);
            }
            // Otherwise try to use quest's current speaker text table:
            var fieldName = tag.Substring(1, tag.Length - 2).Trim();

            // Otherwise try from active node tag dictionary:
            if (NodeTagDictionary != null && NodeTagDictionary.ContainsTag(tag))
            {
                return NodeTagDictionary.dict[tag];
            }

            // Otherwise try from quest tag dictionary:
            if (QuestTagDictionary != null && QuestTagDictionary.ContainsTag(tag))
            {
                return QuestTagDictionary.dict[tag];
            }

            // Otherwise return tag itself :
            return fieldName;
        }

        private static string ReplaceCounterTag( string s, CounterTagType tagType )
        {
            if (string.IsNullOrEmpty(s) || CurrentQuest == null)
            {
                return s;
            }

            var counterName = tagType == CounterTagType.Current || tagType == CounterTagType.AsTime
                ? s.Substring(2, s.Length - 3).Trim()
                : s.Substring(3, s.Length - 4).Trim();

            // Look for counter in current quest:
            var counter = CurrentQuest.GetCounter(counterName);
            if (counter != null)
            {
                return GetCounterTagValue(counter, tagType);
            }

            // Otherwise look for quest by ID:
            var index = counterName.IndexOf(CounterTagQuestNameSeparator);
            if (index > 0)
            {
                var questName = counterName.Substring(0, index);
                counterName = counterName.Substring(index + 1);
                var quest = QuestUtility.GetQuestInstance(questName);
                counter = quest != null ? quest.GetCounter(counterName) : null;
                if (counter != null)
                {
                    return GetCounterTagValue(counter, tagType);
                }
            }
            return s;
        }

        private static string GetCounterTagValue( QuestCounter counter, CounterTagType tagType )
        {
            if (counter == null)
            {
                return string.Empty;
            }
            switch (tagType)
            {
                default:
                case CounterTagType.Current:
                    return counter.currentValue.ToString();
                case CounterTagType.Min:
                    return counter.minValue.ToString();
                case CounterTagType.Max:
                    return counter.maxValue.ToString();
                case CounterTagType.AsTime:
                    return SecondsToTimeString(counter.currentValue);
            }
        }

        /// <summary>
        /// Converts seconds into DD HH:MM:SS time format.
        /// </summary>
        public static string SecondsToTimeString( int seconds )
        {
            if (seconds < 60)
            {
                return seconds.ToString();
            }
            var t = TimeSpan.FromSeconds(seconds);
            if (t.Days > 0)
            {
                return string.Format("{0}d, {1:D2}:{2:D2}:{3:D2}", t.Days, t.Hours, t.Minutes, t.Seconds);
            }
            if (t.Hours > 0)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", new object[] {t.Hours, t.Minutes, t.Seconds});
            }
            return string.Format("{0:D2}:{1:D2}", new object[] {t.Minutes, t.Seconds});
        }

        #endregion
    }
}