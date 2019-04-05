﻿using System;

namespace CineastUnityInterface.CineastAPI.Result
{
    [Serializable]
    public class ContentObject : IComparable<ContentObject>
    {
        public string key;
        public string value;

        public ContentObject()
        {
        }

        public ContentObject(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public int CompareTo(ContentObject other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (ReferenceEquals(null, other)) return 1;

            return string.Compare(value, other.value, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", key, value);
        }

        public static string ArrayToStrig(ContentObject[] array)
        {
            var temp = new string[array.Length];
            for (var i = 0; i < array.Length; i++) temp[i] = array[i].ToString();

            return string.Join(", ", temp);
        }
    }
}