namespace Classe_SAE201_bis.Metier
{
    public class Salarie
    {
        public int SalarieId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string LoginSalarie { get; set; }
        public string RoleSalarie { get; set; }

        public Salarie( int id, string nom, string prenom, string login, string role )
        {
            SalarieId = id;
            Nom = nom;
            Prenom = prenom;
            LoginSalarie = login;
            RoleSalarie = role;
        }

        public bool EstVendeur() => RoleSalarie == "vendeur";
        public bool EstChef() => RoleSalarie == "chef_boulanger";

        public override string ToString() => $"{Prenom} {Nom} ({RoleSalarie})";
    }  
}
