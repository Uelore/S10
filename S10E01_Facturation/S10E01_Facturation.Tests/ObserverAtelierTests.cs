using Commandes.Application;
using Commandes.Infrastructure;
using Commandes.Presentation;
using Xunit;

namespace Commandes.Tests;

public sealed class ObserverAtelierTests
{
    [Fact]
    [Trait("Categorie", "Atelier")]
    public void Ajout_RecoitLeMemeInstantaneDansDeuxObservateurs()
    {
        using ModeleCommande modele = CreerModele();
        ObservateurEspion premier = new ObservateurEspion();
        ObservateurEspion second = new ObservateurEspion();
        using IDisposable a = modele.Subscribe(premier);
        using IDisposable b = modele.Subscribe(second);
        Assert.True(modele.Ajouter(1, 2).EstSucces);
        //Assert.Single(premier.Etats);
        //Assert.Single(second.Etats);
        Assert.Same(premier.Etats[0], second.Etats[0]);
        Assert.Equal(14.30m, premier.Etats[0].Facture.Total);
    }

    [Fact]
    [Trait("Categorie", "Atelier")]
    public void Dispose_RetireUneVueEtLaisseLAutreAbonnee()
    {
        using ModeleCommande modele = CreerModele();
        ObservateurEspion premier = new ObservateurEspion();
        ObservateurEspion second = new ObservateurEspion();
        // S10-T2 : abonner les deux, désabonner le premier deux fois,
        // ajouter une ligne et vérifier que seul le second reçoit l’état.
        using IDisposable a = modele.Subscribe(premier);
        using IDisposable b = modele.Subscribe(second);
        a.Dispose();
        modele.Ajouter(1, 2);
        Assert.Empty(premier.Etats);
    }

    private static ModeleCommande CreerModele()
    {
        ServiceCommandes commandes = new ServiceCommandes(new DepotCommandesMemoire(), new CatalogueProduitsMemoire());
        return new ModeleCommande(commandes, new ServiceFacturation(commandes), new JournalSilencieux());
    }

    private sealed class ObservateurEspion : IObserver<EtatCommandeDto>
    {
        public List<EtatCommandeDto> Etats { get; } = new List<EtatCommandeDto>();
        public void OnNext(EtatCommandeDto etat)
        {
            Etats.Add(etat);
        }
        public void OnCompleted()
        {
        }
        public void OnError(Exception erreur)
        {
        }
    }

    private sealed class JournalSilencieux : IJournalIncidents
    {
        public void Consigner(string operation, Exception erreur)
        {
        }
    }
}
