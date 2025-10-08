using UnityEngine;

public interface ItriggerCheckables 
{
   bool detectsIngredient {  get; set; }

    bool isSlapped { get; set; }

    bool isThrown { get; set; }



    void SetIngredientDetectionStatus(bool DetectsIngredeint);

    void SetIsSlappedStatus(bool IsSlapped);

    void SetIsThrownStatus(bool IsThrown);

}
