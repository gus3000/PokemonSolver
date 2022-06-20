namespace PokemonSolver.Encrypted
{
    
    
    public class PokemonEncryptedData
    {
        //Growth
        //TODO species
        //TODO item held
        private int Experience { get; set; }
        //TODO pp bonuses
        private int Friendship { get; set; }
        
        
        //Attacks
        //TODO moves
        private int PP1 { get; set; }
        private int PP2 { get; set; }
        private int PP3 { get; set; }
        private int PP4 { get; set; }
        
        //EVs
        //TODO
        
        //Misc
        private int pokerus{ get; set; }
        //TODO met location
        //TODO origins info
        //TODO Ivs, Egg, Ability
        //TODO ribbons
    }
}