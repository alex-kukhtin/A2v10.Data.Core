﻿// Copyright © 2015-2020 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public interface IDataProfiler
{
	IDisposable? Start(String command);
}

