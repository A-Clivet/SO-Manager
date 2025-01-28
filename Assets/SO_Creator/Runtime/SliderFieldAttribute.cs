using UnityEngine;

namespace SOCreatorPackage
{
    public class SliderFieldAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }

        public SliderFieldAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}