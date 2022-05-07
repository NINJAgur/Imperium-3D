using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public GameObject gameManager;
    public Empire currenEmpire;
    public Map Grid;

    /// <summary>
    /// AI for an RTS game typically uses multi-tier AI, where at the highest level broad strategic goals are chosen, 
    /// and at lower levels these are converted into tactics and finally unit actions. Different techniques can be used at each level; 
    /// for example, the highest level will probably be a rule-based system, mid levels may use goal-oriented action planning, and on 
    /// the finest levels you'll be doing things like A* path-finding.
    /// 
    /// AI LEVELS : 
    /// 
    /// 1. AI_OVERSEER  GENERAL COURSE - determines empires' general behaviour take into consideration : 
    ///     a. friendly and known enemy and friendly unit desposition                                       
    ///     b. current relations with other countries                                                       
    ///     c. current research focuses and resources competition
    ///     d. improvement data from previous turns
    ///     
    ///     creates resouce queue needs and sends commands to lower levels to manage Research, Construction and Trade
    /// 
    /// After determining main course - > next levels are calculated
    /// 
    /// ->
    /// 2.  AI_DIP , AI RESOURCES
    ///     
    ///     a. AI_DIP - > according to main course, determines action on diplomacy tab
    ///     b AI_RESOURCES - > with GOAP algorithm gets
    /// 
    /// ->
    /// 3. AI_GROUP - commands 
    ///     once a commands pack is fullfilled, the algorithm returns back to higher level to receive additional stacks 
    ///     
    ///     a. Soldier management - > creates soldier fronts ( relocates units to general locations) to secure commands from top tabs
    ///     b. Worker Management  - > manages worker units according to passed command stacks
    ///     c. Boat management -> manages warship units according to passed command stacks
    ///     
    ///      once all commands from top queues are complete / not enough resources to fullfill, AI sets to final stage    
    /// 
    /// 4. Prediction Stage
    ///     
    ///     AI simulates the next turn to make sure it stays on top :
    ///     if not all params are on top 
    ///         -> store improvement data for next turn
    ///     else
    ///         -> finish
    ///  
    /// 
    /// </summary>


    public void Init()
    {
        // AI MAIN COURSE INIT

        AI_OVERSEER _OVERSEER = new AI_OVERSEER();
        _OVERSEER.InitData(gameManager, currenEmpire, Grid);

    }

    
}
