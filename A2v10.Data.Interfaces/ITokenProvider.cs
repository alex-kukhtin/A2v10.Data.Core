// Copyright © 2020 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface ITokenProvider
{
	String GenerateToken(Guid accessToken);
}

