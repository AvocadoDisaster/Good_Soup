using UnityEngine;

public interface ItriggerCheckables 
{
   bool detectsIngredient {  get; set; }

    bool isSlapped { get; set; }

    bool isThrown { get; set; }

    bool isTransportingIngredient { get; set; }

    bool isRallied { get; set; }

    void SetIngredientDetectionStatus(bool DetectsIngredeint);

    void SetIsSlappedStatus(bool IsSlapped);

    void SetIsRallied(bool IsRallied);

    void SetIsThrown(bool IsThrown);

    

    void SetIsTransportingIngredient(bool istransportingIngredient);

}
