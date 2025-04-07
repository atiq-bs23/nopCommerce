using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Checkout;

public partial record CheckoutPickupPointsModel : BaseNopModel
{
    public CheckoutPickupPointsModel()
    {
        Warnings = [];
        PickupPoints = [];
    }

    public IList<string> Warnings { get; set; }

    public IList<CheckoutPickupPointModel> PickupPoints { get; set; }
    public bool AllowPickupInStore { get; set; }
    public bool PickupInStore { get; set; }
    public bool PickupInStoreOnly { get; set; }
    public bool DisplayPickupPointsOnMap { get; set; }
    public string GoogleMapsApiKey { get; set; }
}