﻿// Copyright © 2019-2020 Oleksandr Kukhtin. All rights reserved.

using System.IO;

namespace A2v10.Data.Interfaces;
public interface IExternalDataWriter
{
	void Write(Stream stream);
}

