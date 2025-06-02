using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Helpers;
using Faker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Helper;

public partial class ContactInfo : ObservableObject
{
    [ObservableProperty]
    public partial string PhoneNumber { get; set; }

    [ObservableProperty]
    public partial string LinkedInProfile { get; set; }
}

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum UserStatus
{
    [Description("激活")]
    Active,

    [Description("未激活")]
    Inactive,

    [Description("待激活")]
    Pending
}

public partial class FakerData : ObservableValidator
{
    [NotifyDataErrorInfo]
    [ObservableProperty]
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial UserStatus Status { get; set; }

    [ObservableProperty]
    public partial bool IsVerified { get; set; }

    [NotifyDataErrorInfo]
    [ObservableProperty]
    [Required(ErrorMessage = "First Name is required")]
    public partial string FirstName { get; set; }

    [NotifyDataErrorInfo]
    [ObservableProperty]
    [Required(ErrorMessage = "Last Name is required")]
    public partial string LastName { get; set; }

    [NotifyDataErrorInfo]
    [ObservableProperty]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Address { get; set; }

    [ObservableProperty]
    public partial string City { get; set; }

    [ObservableProperty]
    public partial ContactInfo Contact { get; set; }

    [ObservableProperty]
    public partial DateTime DateOfBirth { get; set; }

    [ObservableProperty]
    public partial string ZipCode { get; set; }

    [ObservableProperty]
    public partial string Country { get; set; }

    [ObservableProperty]
    public partial string Lorem { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    [ObservableProperty]
    public partial Brush Background { get; set; }

    partial void OnFirstNameChanged(string value)
    {
    }

    public FakerData()
    {
        Age = Faker.RandomNumber.Next(0, 120); // 随机生成年龄
        FirstName = Faker.Name.First();        // 随机生成名字
        LastName = Faker.Name.Last();          // 随机生成姓氏
        Email = Faker.Internet.Email();        // 随机生成电子邮件
        Address = Faker.Address.StreetAddress(); // 随机生成街道地址
        City = Faker.Address.City();           // 随机生成城市
        DateOfBirth = Faker.Identification.DateOfBirth(); // 随机生成出生日期
        ZipCode = Faker.Address.ZipCode();     // 随机生成邮政编码
        Country = Faker.Address.Country();     // 随机生成国家
        Lorem = Faker.Lorem.Sentence(5);
        Status = Faker.Enum.Random<UserStatus>();
        IsVerified = Faker.Boolean.Random();
        Contact = new ContactInfo
        {
            PhoneNumber = Faker.Phone.Number(), // 随机生成电话号码
            LinkedInProfile = Faker.Internet.Url() // 随机生成 LinkedIn URL
        };

        // 添加随机颜色生成代码
        // 生成随机的RGB值
        byte r = (byte)Faker.RandomNumber.Next(0, 256);
        byte g = (byte)Faker.RandomNumber.Next(0, 256);
        byte b = (byte)Faker.RandomNumber.Next(0, 256);

        // 创建颜色和画刷
        Color randomColor = Color.FromRgb(r, g, b);
        Background = new SolidColorBrush(randomColor);
    }

    [RelayCommand]
    void ShowSelf()
    {
        MessageBox.Show($"{FirstName}-{LastName}");
    }

    public override string ToString()
    {
        return $"{FirstName}";
    }
}

public partial class TreeFakerData : ObservableObject
{
    [ObservableProperty]
    public partial string Node { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TreeFakerData> Children { get; set; }

    public TreeFakerData()
    {
        Children = [];
    }
}