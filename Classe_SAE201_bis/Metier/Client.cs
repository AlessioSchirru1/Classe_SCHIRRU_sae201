using Classe_SAE201_bis.Metier;
using System.Collections.Generic;

namespace Classe_SAE201_bis.Metier
{
    public class Client
    {
        public int ClientId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Telephone { get; set; }
        public string Mail { get; set; }
        public List<Commande> Commandes { get; set; }

        public Client( int id, string nom, string prenom, string telephone, string mail )
        {
            ClientId = id;
            Nom = nom;
            Prenom = prenom;
            Telephone = telephone;
            Mail = mail;
            Commandes = new List<Commande>();
        }

        public Client( string nom, string prenom, string telephone, string mail )
            : this(0, nom, prenom, telephone, mail) { }

        public override string ToString()
        {
            return $"{Nom} {Prenom} — {Telephone}";
        }
    }
}
