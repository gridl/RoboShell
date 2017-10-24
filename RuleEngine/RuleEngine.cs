﻿using RoboLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RuleEngineNet
{
    public class RuleEngine
    {
        public State State { get; private set; }
        public List<Rule> KnowlegeBase { get; private set; }
        public RuleEngine()
        {
            State = new State();
            KnowlegeBase = new List<Rule>();
        }

        public RuleEngine(List<Rule> KB, State S)
        {
            this.KnowlegeBase = KB;
            this.State = S;
        }

        public static RuleEngine LoadXml(XDocument xdoc)
        {
            var KB =
                (from x in xdoc.Descendants("Rules")
                 select Rule.LoadXml(x)).ToList();
            var S = new State();
            var t = from x in xdoc.Descendants("State").First().Descendants()
                    select x;
            foreach(var v in t)
            {
                S.Add(v.Attribute("Name").Value, v.Attribute("Value").Value);
            }
            return new RuleEngine(KB,S);
        }

        public void SetSpeaker(ISpeaker spk)
        {
            Say.Speaker = spk;
        }

        public IEnumerable<Rule> GetConflictSet(State S)
        {
            return from x in KnowlegeBase
                   where x.Active && x.If.Eval(S).AsBool()
                   orderby x.Priority
                   select x;
        }

        public bool Step()
        {
            var cs = GetConflictSet(State);
            if (cs.Count() > 0)
            {
                var rule = cs.First();
                rule.Then.Execute(State);
                rule.Active = false;
                return true;
            }
            else return false;
        }

        public void Run()
        {
            while (Step()) ;
        }

    }
}