using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    // Clase base abstracta
    public class MetodosdePago
    {
        [Key]
        public int Id { get; set; }
        public IList<Compra> Compras { get; set; }
        public IList<Reposicion> Reposiciones { get; set; }
        public IList<Subasta> Subastas { get; set; }


        public MetodosdePago() { }

        public override bool Equals(object? obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }

}