using UnityEngine;

namespace CustomAttributes
{
    /// <summary>
    /// Attribut personnalisé permettant de montrer ou cacher des champs dans l'inspecteur en fonction d'une condition booléenne.
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionField;

        public ShowIfAttribute(string conditionField)
        {
            ConditionField = conditionField;
        }
    }
}