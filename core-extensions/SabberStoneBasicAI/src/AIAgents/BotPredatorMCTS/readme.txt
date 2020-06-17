- Add the following include to  .\core-extensions\SabberStoneCoreAi\SabberStoneCoreAi.csproj

	<PackageReference Include="Newtonsoft.Json" Version="11.0.2"/>

- Place the Bigramms folder into
	
	.\src\

- Use the AgentFactory to create an agent depending on the current deck:
	
	AgentFactory factory = Activator.CreateInstance(typeof(AgentFactory)) as AgentFactory;
	player1 = factory.GetAgent(factory.hero, factory.deck, AgentFactory.AgentType.PredatorMCTS);

- In case the agent is not allowed to play its own deck change factory.hero and factory.deck to the appropriate deck
