using Classe_SAE201_bis.Metier;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Classe_SAE201_bis.Metier
{
    public class Commande
    {
        public int CommandeId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateRetrait { get; set; }
        public double Acompte { get; set; }
        public bool EstPrete { get; set; }
        public bool EstRecuperee { get; set; }
        public double Total { get; set; }
        public int NbPersonnes { get; set; }
        public DateTime? DateEvenement { get; set; }
        public Client Client { get; set; }
        public CategorieEvenement CategorieEvenement { get; set; }
        public List<LigneCommande> Lignes { get; set; }

        public Commande( int id, DateTime dateRetrait, int nbPersonnes,
                        Client client, CategorieEvenement categorieEvenement = null )
        {
            CommandeId = id;
            DateCreation = DateTime.Now;
            DateRetrait = dateRetrait;
            NbPersonnes = nbPersonnes;
            Client = client;
            CategorieEvenement = categorieEvenement;
            EstPrete = false;
            EstRecuperee = false;
            Lignes = new List<LigneCommande>();
        }

        public Commande( DateTime dateRetrait, int nbPersonnes, Client client )
            : this(0, dateRetrait, nbPersonnes, client) { }

        public double CalculerTotal()
        {
            return Lignes.Sum(l => l.CalculerSousTotal());
        }

        public void MarquerPrete()
        {
            EstPrete = true;
        }

        public void MarquerRecuperee()
        {
            EstRecuperee = true;
        }

        public override string ToString()
        {
            return $"Commande #{CommandeId} — {Client} — retrait {DateRetrait:dd/MM/yyyy}";
        }
    }
}
