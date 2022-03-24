using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.Model
{
	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(ArrayOfBasicPropertyInfo));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (ArrayOfBasicPropertyInfo)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "Text")]
	public class Text
	{

		[XmlAttribute(AttributeName = "TextFormat")]
		public string TextFormat { get; set; }

		[XmlAttribute(AttributeName = "ID")]
		public int ID { get; set; }

		[XmlText]
		public string text { get; set; }
	}

	[XmlRoot(ElementName = "Paragraph")]
	public class Paragraph
	{

		[XmlElement(ElementName = "Text")]
		public Text Text { get; set; }

		[XmlElement(ElementName = "URL")]
		public string URL { get; set; }

		[XmlAttribute(AttributeName = "CreatorID")]
		public int CreatorID { get; set; }

		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlText]
		public string text { get; set; }
	}

	[XmlRoot(ElementName = "SubSection")]
	public class SubSection
	{

		[XmlElement(ElementName = "Paragraph")]
		public List<Paragraph> Paragraph { get; set; }

		[XmlAttribute(AttributeName = "SubTitle")]
		public string SubTitle { get; set; }

		[XmlAttribute(AttributeName = "Category")]
		public string Category { get; set; }

		[XmlAttribute(AttributeName = "CategoryCode")]
		public int CategoryCode { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "VendorMessage")]
	public class VendorMessage
	{

		[XmlElement(ElementName = "SubSection")]
		public List<SubSection> SubSection { get; set; }

		[XmlAttribute(AttributeName = "Title")]
		public string Title { get; set; }

		[XmlAttribute(AttributeName = "InfoType")]
		public int InfoType { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "VendorMessages")]
	public class VendorMessages
	{

		[XmlElement(ElementName = "VendorMessage")]
		public List<VendorMessage> VendorMessage { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Position")]
	public class Position
	{

		[XmlAttribute(AttributeName = "Latitude")]
		public double Latitude { get; set; }

		[XmlAttribute(AttributeName = "Longitude")]
		public double Longitude { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName = "CountryName")]
	public class CountryName
	{

		[XmlAttribute(AttributeName = "Code")]
		public string Code { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Address")]
	public class Address
	{

		[XmlElement(ElementName = "AddressLine")]
		public List<string> AddressLine { get; set; }

		[XmlElement(ElementName = "CityName")]
		public string CityName { get; set; }

		[XmlElement(ElementName = "PostalCode")]
		public int PostalCode { get; set; }

		[XmlElement(ElementName = "StateProv")]
		public string StateProv { get; set; }

		[XmlElement(ElementName = "CountryName")]
		public CountryName CountryName { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "ContactNumber")]
	public class ContactNumber
	{

		[XmlAttribute(AttributeName = "PhoneNumber")]
		public string PhoneNumber { get; set; }

		[XmlAttribute(AttributeName = "PhoneTechType")]
		public string PhoneTechType { get; set; }
	}

	[XmlRoot(ElementName = "ContactNumbers")]
	public class ContactNumbers
	{

		[XmlElement(ElementName = "ContactNumber")]
		public List<ContactNumber> ContactNumber { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Award")]
	public class Award
	{

		[XmlAttribute(AttributeName = "Provider")]
		public string Provider { get; set; }

		[XmlAttribute(AttributeName = "Rating")]
		public double Rating { get; set; }

		[XmlAttribute(AttributeName = "ReviewURL")]
		public object ReviewURL { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName = "Policy")]
	public class Policy
	{

		[XmlAttribute(AttributeName = "CheckInTime")]
		public DateTime CheckInTime { get; set; }

		[XmlAttribute(AttributeName = "CheckOutTime")]
		public DateTime CheckOutTime { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName = "HotelTheme")]
	public class HotelTheme
	{

		[XmlAttribute(AttributeName = "ThemeId")]
		public int ThemeId { get; set; }

		[XmlAttribute(AttributeName = "ThemeName")]
		public string ThemeName { get; set; }
	}

	[XmlRoot(ElementName = "HotelThemes")]
	public class HotelThemes
	{

		[XmlElement(ElementName = "HotelTheme")]
		public HotelTheme HotelTheme { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "RoomFacility")]
	public class RoomFacility
	{

		[XmlElement(ElementName = "FacilityId")]
		public int FacilityId { get; set; }

		[XmlElement(ElementName = "FacilityName")]
		public string FacilityName { get; set; }
	}

	[XmlRoot(ElementName = "Faciltities")]
	public class Faciltities
	{

		[XmlElement(ElementName = "RoomFacility")]
		public List<RoomFacility> RoomFacility { get; set; }
	}

	[XmlRoot(ElementName = "RoomImage")]
	public class RoomImage
	{

		[XmlElement(ElementName = "ImageUrl")]
		public string ImageUrl { get; set; }

		[XmlElement(ElementName = "ImageType")]
		public int ImageType { get; set; }
	}

	[XmlRoot(ElementName = "RoomImages")]
	public class RoomImages
	{

		[XmlElement(ElementName = "RoomImage")]
		public List<RoomImage> RoomImage { get; set; }
	}

	[XmlRoot(ElementName = "RoomDescription")]
	public class RoomDescription
	{

		[XmlElement(ElementName = "SubSection")]
		public List<SubSection> SubSection { get; set; }
	}

	[XmlRoot(ElementName = "Room")]
	public class Room
	{

		[XmlElement(ElementName = "BedTypes")]
		public BedTypes BedTypes { get; set; }

		[XmlElement(ElementName = "RoomImages")]
		public RoomImages RoomImages { get; set; }

		[XmlElement(ElementName = "RoomDescription")]
		public RoomDescription RoomDescription { get; set; }

		[XmlElement(ElementName = "HotelCode")]
		public int HotelCode { get; set; }

		[XmlElement(ElementName = "RoomTypeName")]
		public string RoomTypeName { get; set; }

		[XmlElement(ElementName = "RoomId")]
		public int RoomId { get; set; }

		[XmlElement(ElementName = "MaxOccupancy")]
		public int MaxOccupancy { get; set; }

		[XmlElement(ElementName = "MinOccupancy")]
		public int MinOccupancy { get; set; }

		[XmlElement(ElementName = "AllowExtraBed")]
		public string AllowExtraBed { get; set; }

		[XmlElement(ElementName = "NoOfExtraBed")]
		public int NoOfExtraBed { get; set; }

		[XmlElement(ElementName = "RoomSizeFeet")]
		public int RoomSizeFeet { get; set; }

		[XmlElement(ElementName = "RoomSizeMeter")]
		public object RoomSizeMeter { get; set; }

		[XmlElement(ElementName = "Faciltities")]
		public Faciltities Faciltities { get; set; }

		[XmlElement(ElementName = "RoomViews")]
		public RoomViews RoomViews { get; set; }
	}

	[XmlRoot(ElementName = "BedType")]
	public class BedType
	{

		[XmlElement(ElementName = "BedID")]
		public int BedID { get; set; }

		[XmlElement(ElementName = "BedName")]
		public string BedName { get; set; }

		[XmlElement(ElementName = "BedSize")]
		public object BedSize { get; set; }

		[XmlElement(ElementName = "Quantity")]
		public int Quantity { get; set; }
	}

	[XmlRoot(ElementName = "BedTypes")]
	public class BedTypes
	{

		[XmlElement(ElementName = "BedType")]
		public BedType BedType { get; set; }
	}

	[XmlRoot(ElementName = "RoomView")]
	public class RoomView
	{

		[XmlElement(ElementName = "ViewID")]
		public int ViewID { get; set; }

		[XmlElement(ElementName = "ViewName")]
		public string ViewName { get; set; }
	}

	[XmlRoot(ElementName = "RoomViews")]
	public class RoomViews
	{

		[XmlElement(ElementName = "RoomView")]
		public RoomView RoomView { get; set; }
	}

	[XmlRoot(ElementName = "Rooms")]
	public class Rooms
	{

		[XmlElement(ElementName = "Room")]
		public List<Room> Room { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "Attribute")]
	public class AttributesX
	{

		[XmlAttribute(AttributeName = "AttributeName")]
		public string AttributeName { get; set; }

		[XmlAttribute(AttributeName = "AttributeType")]
		public string AttributeType { get; set; }
	}

	[XmlRoot(ElementName = "Attributes")]
	public class Attributes
	{

		[XmlElement(ElementName = "Attribute")]
		public List<AttributesX> Attribute { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "BasicPropertyInfo")]
	public class BasicPropertyInfo
	{

		[XmlElement(ElementName = "VendorMessages")]
		public VendorMessages VendorMessages { get; set; }

		[XmlElement(ElementName = "Position")]
		public Position Position { get; set; }

		[XmlElement(ElementName = "Address")]
		public Address Address { get; set; }

		[XmlElement(ElementName = "ContactNumbers")]
		public ContactNumbers ContactNumbers { get; set; }

		[XmlElement(ElementName = "Award")]
		public Award Award { get; set; }

		[XmlElement(ElementName = "Policy")]
		public Policy Policy { get; set; }

		[XmlElement(ElementName = "HotelThemes")]
		public HotelThemes HotelThemes { get; set; }

		[XmlElement(ElementName = "Rooms")]
		public Rooms Rooms { get; set; }

		[XmlElement(ElementName = "Attributes")]
		public Attributes Attributes { get; set; }

		[XmlAttribute(AttributeName = "BrandCode")]
		public int BrandCode { get; set; }

		[XmlAttribute(AttributeName = "TBOHotelCode")]
		public int TBOHotelCode { get; set; }

		[XmlAttribute(AttributeName = "HotelCityCode")]
		public int HotelCityCode { get; set; }

		[XmlAttribute(AttributeName = "HotelName")]
		public string HotelName { get; set; }

		[XmlAttribute(AttributeName = "NoOfRooms")]
		public int NoOfRooms { get; set; }

		[XmlAttribute(AttributeName = "NoOfFloors")]
		public int NoOfFloors { get; set; }

		[XmlAttribute(AttributeName = "BuiltYear")]
		public int BuiltYear { get; set; }

		[XmlAttribute(AttributeName = "RenovationYear")]
		public int RenovationYear { get; set; }

		[XmlAttribute(AttributeName = "HotelCategoryId")]
		public int HotelCategoryId { get; set; }

		[XmlAttribute(AttributeName = "HotelCategoryName")]
		public string HotelCategoryName { get; set; }

		[XmlAttribute(AttributeName = "IsHalal")]
		public bool IsHalal { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "ArrayOfBasicPropertyInfo")]
	public class ArrayOfBasicPropertyInfo
	{

		[XmlElement(ElementName = "BasicPropertyInfo")]
		public BasicPropertyInfo BasicPropertyInfo { get; set; }

		[XmlAttribute(AttributeName = "xsd")]
		public string Xsd { get; set; }

		[XmlAttribute(AttributeName = "xsi")]
		public string Xsi { get; set; }

		[XmlText]
		public string Text { get; set; }
	}


}