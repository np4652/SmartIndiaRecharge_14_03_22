using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAppControllerShoping
    {
        Task<IActionResult> GetShoppingMenu([FromBody] AppRequest appRequest);
        Task<IActionResult> GetProducts([FromBody] ProductFilterRequest appRequest);
        Task<IActionResult> GetFilteredProducts([FromBody] ExtendProductFilterRequest appRequest);
        Task<IActionResult> ProductDescription([FromBody] ProductDescriptionRequest appRequest);
        Task<IActionResult> OnChnageFilter([FromBody] OnFilterChnageRequest appRequest);
        Task<IActionResult> AddToCart([FromBody] AddToCartReq appRequest);
        Task<IActionResult> CartDetail([FromBody] AppSessionReq appRequest);
        Task<IActionResult> ItemInCart([FromBody] AppSessionReq appRequest);
        Task<IActionResult>  ProceedToPay([FromBody] ProceedToPayReq appRequest);
        Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartReq appRequest);
        Task<IActionResult> ChangeQuantity([FromBody] ChangeQuantityReq appRequest);
        Task<IActionResult> PlaceOrder([FromBody] PlaceOrderReq appRequest);
        Task<IActionResult> Order([FromBody] OrderDetailReq appRequest);
        Task<IActionResult> OrderDetail([FromBody] OrderDetailReq appRequest);
        Task<IActionResult> ChangeOrderStatus([FromBody] ChangeOrderStatusReq appRequest);
        Task<IActionResult> GetShippingAddresses([FromBody] GetShippingAddressesReq appRequest);
        Task<IActionResult> AddShippingAddress([FromBody] AddAddressRequest appRequest);
        Task<IActionResult> AllAvailableFilter([FromBody] AllAvailableFilterReq appRequest);
        Task<IActionResult> GetTrendingProducts([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetNewArrivalProducts([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetStates([FromBody] GetStatesReq appRequest);
        Task<IActionResult> GetCities([FromBody] CityReq appRequest);
        Task<IActionResult> GetAreabyPincodeRequest([FromBody] GetAreabyPincodeRequest appRequest);
    }
}
