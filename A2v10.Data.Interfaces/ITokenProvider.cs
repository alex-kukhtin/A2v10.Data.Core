// Copyright © 2020 Alex Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface ITokenProvider
{
	String GenerateToken(Guid accessToken);
}

