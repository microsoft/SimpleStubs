namespace HelloApp
{
    public class Location
    {
        protected bool Equals(Location other)
        {
            return string.Equals(Country, other.Country) && string.Equals(City, other.City);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Country != null ? Country.GetHashCode() : 0)*397) ^ (City != null ? City.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Location left, Location right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Location left, Location right)
        {
            return !Equals(left, right);
        }

        public Location(string country, string city)
        {
            Country = country;
            City = city;
        }

        public string Country { get;  }
        public string City { get; }
    }
}