using OpenTK;

namespace skystride.vendor.collision
{
    internal class AABB
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }

        public Vector3 Min => this.Position - (this.Size / 2f);
        public Vector3 Max => this.Position + (this.Size / 2f);

        public object Owner { get; set; }

        public AABB(Vector3 position, Vector3 size, object owner = null)
        {
            this.Position = position;
            this.Size = size;
            this.Owner = owner;
        }

        public bool Intersects(AABB element)
        {
            // Check for no overlap on the X-axis
            if (this.Max.X < element.Min.X || this.Min.X > element.Max.X)
            {
                return false;
            }

            // Check for no overlap on the Y-axis
            if (this.Max.Y < element.Min.Y || this.Min.Y > element.Max.Y)
            {
                return false;
            }

            // Check for no overlap on the Z-axis
            if (this.Max.Z < element.Min.Z || this.Min.Z > element.Max.Z)
            {
                return false;
            }

            // If there's no separation on all three axes, the boxes must be colliding
            return true;
        }
    }
}
