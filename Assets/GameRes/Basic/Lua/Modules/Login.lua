Login = {}

function Login:Initialize()
    local login = GameObject.Find("UIRoot/LoginPanel(Clone)");
	local text = login.Find("Text").transform:GetComponent("Text");
	text.text = "Hello Lua!"
end