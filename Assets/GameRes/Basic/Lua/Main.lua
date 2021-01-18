require("Basic/Lua/Core/Define")

Main = {}

function Main:Initialize()
	Log.l("Main Initialize")
	local login = GameObject.Find("UIRoot/LoginPanel(Clone)");
	local text = login.Find("Text").transform:GetComponent("Text");
	text.text = "Hello Lua!"
end