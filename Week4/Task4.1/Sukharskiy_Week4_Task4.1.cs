using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;


namespace EntityParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Testing Start:\n");
            var objectsParser = new Parser();
            Dictionary<Type, Func<Match, MyEntity>> delDictionary = new Dictionary<Type, Func<Match, MyEntity>>
            {
                [typeof(Person)] = Person.CreatePerson,
                [typeof(FootballClub)] = FootballClub.CreateClub,
                [typeof(Company)] = Company.CreateCompany,
                [typeof(BasketballClub)] = BasketballClub.CreateClub,
                [typeof(Plane)]= Plane.CreatePlane,
                [typeof(Phone)]=Phone.CreatePhone,
                [typeof(Gun)]=Gun.CreateGun
            };

            const string text = "Mrs. Jessica Brown was born on 1999/12/31."+
                            "Mr. John Smith was born on 2001/11/03."+
                            "Mr. John Smith was born on 2001/11/03." +
                            "Last year winner FC. Dynamo from Kyiv Ukraine 1927 won against AS. Milan from Milan Italy 1899." +
                            " Company Microsoft (Information Technology, US) was closed. " + 
                            "Share price of Microsoft (Information Technology, US) dropped to $100." +
                            "Company Google (IT, US) bought a new product."+
                            "LA 'Lakers' (NBA) won against GS 'Warriors' (NBA). Their next mathc will be played at "+
                            "Staples center against NY 'Knicks' (NBA). LA 'Lakers' (NBA) are the best."+
                            "Boing 747 'Maria' crashed with 144 passengers on it. AN 124 'Ruslan' is the largest" 
                            + " serial cargo aircraft in the world and can fly with 200 passengers."+
                            "iPhone '6S' with 16GB of memory costs 400$ while new HTC 'One' with 32GB memory card costs 200$."+
                            "'AK-47' was created in 1947/08/27 by Mikhail Kalashnikov.";

            var objects = objectsParser.Parse(text, delDictionary);

            Console.WriteLine("Objects:");
            objectsParser.ShowArr( objectsParser.RemoveD(objects) );


            Console.WriteLine("\nProperties info: ");
            Type[] types = { typeof(Person), typeof(Company), typeof(FootballClub), typeof(Plane), typeof(BasketballClub), typeof(Phone), typeof(Gun)};
            var propertiesInfo =  objectsParser.GetPropertiesCollection(types);
            foreach(KeyValuePair<string,string> entry in propertiesInfo)
            {
                Console.WriteLine("<" + entry.Key + ", " + entry.Value + ">");
            }

            Console.WriteLine("\nProperties statistics: ");
            var statisticsDict = objectsParser.GetStatistics(propertiesInfo);
            
            Console.WriteLine("\nTesting END.");
            Console.ReadLine();
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct,
                       AllowMultiple = true)
    ]
    public sealed class RegularExpressionAttribute : Attribute
    {
        public string epression { get; set; }
        public RegularExpressionAttribute(string exp) { epression = exp; }
    }

    class MyEntity : IComparable<MyEntity>
    {
        public MyEntity() { }

        public int CompareTo(MyEntity other)
        {
            return this.CompareTo(other);
        }
    }

    class Parser
    {
        public Parser() { }

        public List<MyEntity> Parse(string text, Dictionary<Type, Func<Match, MyEntity>> delDictionary)
        {
            var objectsList = new List<MyEntity>();
            foreach (KeyValuePair<Type, Func<Match, MyEntity>> entry in delDictionary)
            {
                var objectsMatches = Regex.Matches(text, GetAttributeValue(entry.Key));
                for (int i = 0; i < objectsMatches.Count; i++)
                {
                    Match match = objectsMatches[i];
                    var obj = delDictionary[entry.Key].DynamicInvoke(match);
                    var other = obj as MyEntity;
                    objectsList.Add(other);
                }
            }
            return objectsList;
        }

        public List<MyEntity> RemoveD(List<MyEntity> list)
        {
            var objectsList = new List<MyEntity>();
            objectsList.Add(list.First());
            foreach (MyEntity obj in list)
            {
                if (!IsIn(objectsList, obj))
                    objectsList.Add(obj);
            }
            return objectsList;
        }

        public bool IsIn(List<MyEntity> list, MyEntity obj)
        {
            foreach (MyEntity item in list)
            {
                if (obj.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public string GetAttributeValue(Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute<RegularExpressionAttribute>().epression;
        }

        public Dictionary<string, string> GetPropertiesCollection(Type[] types)
        {
            var dict = new Dictionary<string, string>();
            foreach (Type t in types)
            {
                foreach (var prop in t.GetTypeInfo().DeclaredProperties)
                    dict.Add(prop.Name, prop.PropertyType.Name);
            }
            dict.Distinct();
            return dict;
        }

        public Dictionary<string, int> GetStatistics(Dictionary<string, string> dict)
        {
            var statisticsDict = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> entry in dict)
            {
                if (!statisticsDict.ContainsKey(entry.Value))
                {
                    int count = dict.Count(kv => kv.Value.Contains(entry.Value));
                    statisticsDict.Add(entry.Value, count);
                }
            }
            var items = from pair in statisticsDict orderby pair.Value descending select pair;
            foreach (var item in items)
            {
                Console.WriteLine(item.Key + " - " + item.Value);
            }
            return statisticsDict;
        }

        public void ShowArr(List<MyEntity> arr)
        {
            foreach (object i in arr)
            {
                Console.WriteLine("{0}", i);
            }
        }

    }

    class Name : IComparable<Name>
    {
        private string firstName { get; set; }
        private string lastName { get; set; }

        public Name(string fN, string lN)
        {
            firstName = fN;
            lastName = lN;
        }
        public int CompareTo(Name other)
        {
            var thisName = $"{lastName}{firstName}".ToLower();
            var otherName = $"{other.lastName}{other.firstName}".ToLower();
            return thisName.CompareTo(otherName);
        }
        public bool Equals(Name other)
        {
            return firstName == other.firstName && lastName == other.lastName;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Name))
                return false;
            var other = (Name)obj;
            return Equals(other);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(firstName, lastName).GetHashCode();
        }

        public override string ToString()
        {
            return firstName + ", " + lastName;
        }
    }

    [RegularExpressionAttribute(@"(Mr\.|Mrs\.|Ms\.)\s(\w+)\s(\w+)\swas\sborn\son\s(\d{4}/\d\d/\d\d)")]
    class Person : MyEntity
    {
        public Name fullName { get; private set; }
        private char myGender { get; set; }
        private DateTime birthDate { get; set; }
        private int myAge { get; set; }


        public Person(string firstName, string lastName, char gender, DateTime data)
        {
            fullName = new Name(firstName, lastName);
            birthDate = new DateTime(data.Year, data.Month, data.Day);
            myGender = gender;
            myAge = getAge(data);
        }

        public int getAge(DateTime data)
        {
            DateTime today = DateTime.Today;
            if (today.Month >= data.Month && today.Day >= data.Day)
                return today.Year - data.Year;
            return today.Year - data.Year - 1;
        }
        protected virtual bool EqualPersons(Person i)
        {
            return fullName.Equals(i.fullName) && myGender == i.myGender && birthDate == i.birthDate;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Person;

            return other != null && EqualPersons(other);
        }
        public int CompareTo(Person other)
        {
            if (!myGender.Equals(other.myGender))
                return myGender.CompareTo(other.myGender);

            if (!fullName.Equals(other.fullName))
                return fullName.CompareTo(other.fullName);

            if (birthDate != other.birthDate)
                return birthDate.CompareTo(other.birthDate);

            return 0;
        }
        public override int GetHashCode()
        {
            return Tuple.Create(fullName, birthDate, myGender).GetHashCode();
        }

        static public Person CreatePerson(Match match)
        {
            var gender = (match.Groups[1].Value == "Mr.") ? 'm' : 'f';
            var firstName = match.Groups[2].Value;
            var lastName = match.Groups[3].Value;
            var birthDate = DateTime.Parse(match.Groups[4].Value);

            return new Person(firstName, lastName, gender, birthDate);
        }

        public override string ToString()
        {
            return fullName + " ( " + myGender + ", " + myAge + " ) ";
        }

    }

    [RegularExpressionAttribute(@"(FC\.|AS\.)\s(\w+)\s+(\w+)\s+(\w+)\s+(\w+)\s+(\d{4})")]
    class FootballClub : MyEntity
    {
        private string shortCode { get; set; }
        private string name { get; set; }
        private string country { get; set; }
        private string city { get; set; }
        private int year { get; set; }

        public FootballClub(string ShortCode, string FCname, string Country, string City, int Year)
        {
            shortCode = ShortCode;
            name = FCname;
            country = Country;
            city = City;
            year = Year;
        }

        protected virtual bool EqualClubs(FootballClub i)
        {
            return name == i.name && city == i.city && country == i.country && year == i.year;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FootballClub;

            return other != null && EqualClubs(other);
        }

        public int CompareTo(FootballClub other)
        {
            if (!year.Equals(other.year))
                return year.CompareTo(other.year);

            if (!name.Equals(other.name))
                return name.CompareTo(other.name);

            if (country != other.country)
                return country.CompareTo(other.country);
            if (city != other.city)
                return city.CompareTo(other.city);
            return 0;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(name, country, city, year).GetHashCode();
        }

        static public FootballClub CreateClub(Match match)
        {
            var sc = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var country = match.Groups[5].Value;
            var city = match.Groups[4].Value;
            var year = Int32.Parse(match.Groups[6].Value);

            return new FootballClub(sc, name, country, city, year);
        }

        public override string ToString()
        {
            return $"{this.shortCode} {this.name} ( {this.city} , {this.country}, {this.year} )";
        }

    }

    [RegularExpressionAttribute(@"(\w+\s+)*?(\w+)\s+\(([^,]*)[^)]([^)]*)\)(\w+\s+)*?")]
    class Company : MyEntity
    {
        private string companyName { get; set; }
        private string industryName { get; set; }
        private string companyCode { get; set; }

        public Company(string name, string industry, string code)
        {
            companyName = name;
            industryName = industry;
            companyCode = code;
        }

        protected virtual bool EqualCompanies(Company i)
        {
            return companyName == i.companyName && industryName == i.industryName && companyCode == i.companyCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Company;

            return other != null && EqualCompanies(other);
        }

        public int CompareTo(Company other)
        {
            if (!companyName.Equals(other.companyName))
                return companyName.CompareTo(other.companyName);
            return 0;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(companyName, industryName, companyCode).GetHashCode();
        }

        static public Company CreateCompany(Match match)
        {
            var name = match.Groups[2].Value;
            var ind = match.Groups[3].Value;
            var code = match.Groups[4].Value;

            return new Company(name, ind, code);
        }

        public override string ToString()
        {
            return $"{this.companyName} ( {this.industryName}, {this.companyCode} )";
        }

    }

    [RegularExpressionAttribute(@"(\w+)+\s+\'([^']+)\'\s+\(([^)]+)\)")]
    class BasketballClub : MyEntity
    {
        private string bcName { get; set; }
        private string bcCity { get; set; }
        private string bcLeague { get; set; }

        public BasketballClub(string name, string city, string league)
        {
            bcName = name; bcCity = city; bcLeague = league;
        }
        protected virtual bool EqualClubs(BasketballClub i)
        {
            return bcName == i.bcName && bcCity == i.bcCity && bcLeague == i.bcLeague;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BasketballClub;
            return other != null && EqualClubs(other);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(bcName, bcCity, bcLeague).GetHashCode();
        }
        static public BasketballClub CreateClub(Match match)
        {
            var name = match.Groups[2].Value;
            var city = match.Groups[1].Value;
            var league = match.Groups[3].Value;

            return new BasketballClub(name, city, league);
        }
        public override string ToString()
        {
            return $"{this.bcCity} {this.bcName} ( {this.bcLeague})";
        }
    }

    [RegularExpressionAttribute(@"(\w+)\s+(\d{3}|\d{2})\s+\'([^']+)\'\s+(\w+\s+)*with\s+(\d{1}|\d{2}|\d{3})\s+passengers")]
    class Plane : MyEntity
    {
        string planeName { get; set; }
        int planeCode { get; set; }
        int planeCapacity { get; set; }
        string planeType { get; set; }

        public Plane(string name, int code, int passengers, string type)
        {
            planeName = name; planeCode = code; planeCapacity = passengers; planeType = type;
        }
        protected virtual bool EqualPlanes(Plane i)
        {
            return planeName == i.planeName && planeCode == i.planeCode && planeCapacity == i.planeCapacity && planeType == i.planeType;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Plane;
            return other != null && EqualPlanes(other);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(planeName, planeCode, planeCapacity, planeType).GetHashCode();
        }
        static public Plane CreatePlane(Match match)
        {
            var code = Int32.Parse(match.Groups[2].Value);
            var pass = Int32.Parse(match.Groups[5].Value);
            var type = match.Groups[1].Value;
            var name = match.Groups[3].Value;
            return new Plane(name, code, pass, type);
        }
        public override string ToString()
        {
            return $"{this.planeType} {this.planeCode} {this.planeName}, capacity = {this.planeCapacity}";
        }
    }

    [RegularExpressionAttribute(@"(\w+)\s+\'([^']+)\'\s+with\s+(\d{3}|\d{2}|\d{1})(\w+\s+)+costs\s+(\d{5}|\d{4}|\d{3})\s*")]
    class Phone : MyEntity
    {
        string phoneName { get; set; }
        string phoneModel { get; set; }
        int phoneMemory { get; set; }
        int phonePrice { get; set; }

        public Phone(string name, string model, int memory, int price)
        {
            phoneName = name; phoneModel = model; phoneMemory = memory; phonePrice = price;
        }
        protected virtual bool EqualPhones(Phone i)
        {
            return phoneName == i.phoneName && phoneModel == i.phoneModel && phoneMemory == i.phoneMemory && phonePrice == i.phonePrice;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Phone;
            return other != null && EqualPhones(other);
        }
        public override int GetHashCode()
        {
            return Tuple.Create(phoneName, phoneModel, phoneMemory, phonePrice).GetHashCode();
        }
        static public Phone CreatePhone(Match match)
        {
            var name = match.Groups[1].Value;
            var model = match.Groups[2].Value;
            var memory = Int32.Parse(match.Groups[3].Value);
            var price = Int32.Parse(match.Groups[5].Value);

            return new Phone(name, model, memory, price);
        }
        public override string ToString()
        {
            return $"{this.phoneName} {this.phoneModel}, memory = {this.phoneMemory}GB, price = ${this.phonePrice}";
        }
    }

    [RegularExpressionAttribute(@"\'([^']+)\'\s+(\w+\s+)*(\d{4}/\d\d/\d\d)\s+by\s+(\w+)\s+(\w+)")]
    class Gun : MyEntity
    {
        string gunName { get; set; } 
        Name author { get; set; }
        DateTime creation { get; set; }

        public Gun(string name, Name aName, DateTime date) { gunName = name; author = aName; creation = date; }

        protected virtual bool EqualGuns(Gun i)
        {
            return gunName == i.gunName && author.Equals(i) && creation == i.creation;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Gun;
            return other != null && EqualGuns(other);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(gunName, author, creation).GetHashCode();
        }

        static public Gun CreateGun(Match match)
        {
            var name = match.Groups[1].Value;
            var date = DateTime.Parse(match.Groups[3].Value);

            return new Gun (name, new Name(match.Groups[5].Value, match.Groups[4].Value), date);
        }

        public override string ToString()
        {
            return $"{this.gunName} created by  {this.author} in {this.creation.Year}";
        }
        
    }
    
}

