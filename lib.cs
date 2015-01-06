void MSG(string text)
{
  List<IMyTerminalBlock> antennas = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antennas);
  (antennas[0] as IMyRadioAntenna).SetCustomName(text);
}

string processDirectionName(string direction)
{
  string res = char.ToUpper(direction[0]) + direction.Substring(1).ToLower();

  if (res != "Forward" && res != "Backward" && res != "Up" && res != "Down" && res != "Left" && res != "Right")
  {
    throw new ArgumentException("Invalid direction name: " + direction);
  }

  return res;
}

IMyTerminalBlock getBlock(string blockName)
{
  return getBlock(GridTerminalSystem, blockName);
}

IMyTerminalBlock[] getBlocks(string[] blockNames)
{
  IMyTerminalBlock[] res = new IMyTerminalBlock[blockNames.Length];

  for (int i = 0; i < blockNames.Length; ++i)
  {
    res[i] = getBlock(blockNames[i]);
  }

  return res;
}

public static IMyTerminalBlock getBlock(IMyGridTerminalSystem grid, string blockName)
{
  return grid.GetBlockWithName(blockName);
}

void applyAction(string blockName, string actionName)
{
  IMyTerminalBlock block = getBlock(blockName);
  ITerminalAction action = block.GetActionWithName(actionName);
  action.Apply(block);
}

public static void applyAction(IMyTerminalBlock block, string actionName)
{
  ITerminalAction action = block.GetActionWithName(actionName);
  action.Apply(block);
}

void applyAction(string[] blockNames, string actionName)
{
  for (int i = 0; i < blockNames.Length; ++i)
  {
    IMyTerminalBlock block = getBlock(blockNames[i]);
    ITerminalAction action = block.GetActionWithName(actionName);
    action.Apply(block);
  }
}

bool all(string[] blockNames, Func<IMyTerminalBlock, bool> condition)
{
  return all(getBlocks(blockNames), condition);
}

public static bool all(IMyTerminalBlock[] blocks, Func<IMyTerminalBlock, bool> condition)
{
  for (int i = 0; i < blocks.Length; ++i)
  {
    if (false == condition(blocks[i]))
    {
      return false;
    }
  }

  return true;
}

public static bool any(IMyTerminalBlock[] blocks, Func<IMyTerminalBlock, bool> condition)
{
  for (int i = 0; i < blocks.Length; ++i)
  {
    if (true == condition(blocks[i]))
    {
      return true;
    }
  }

  return false;
}

bool any(string[] blockNames, Func<IMyTerminalBlock, bool> condition)
{
  return any(getBlocks(blockNames), condition);
}

bool isEnabled(string blockName)
{
  return isEnabled(getBlock(blockName));
}

bool isEnabled(IMyTerminalBlock block)
{
  if (block is IMyFunctionalBlock)
  {
    return (block as IMyFunctionalBlock).Enabled;
  }
  else
  {
    return false;
  }
}

void adjustToValue(IMyTerminalBlock block, double targetValue, string increaseMethodName, string decreaseMethodName, Func<IMyTerminalBlock, float> valueGetter)
{
  while (valueGetter(block) > (float)targetValue)
  {
    applyAction(block, decreaseMethodName);
  }

  while (valueGetter(block) < (float)targetValue)
  {
    applyAction(block, increaseMethodName);
  }
}

void adjustToValue(string blockName, double targetValue, string increaseMethodName, string decreaseMethodName, Func<IMyTerminalBlock, float> valueGetter)
{
  adjustToValue(getBlock(blockName), targetValue, increaseMethodName, decreaseMethodName, valueGetter);
}

public static class ValueGetters
{
  public static float thrustOverride(IMyTerminalBlock block)
  {
    if (block is IMyThrust)
    {
      return (block as IMyThrust).ThrustOverride;
    }
    else
    {
      return Single.NaN;
    }
  }

  public static float yaw(IMyTerminalBlock block)
  {
    if (block is IMyGyro)
    {
      return ((IMyGyro)block).Yaw;
    }

    return Single.NaN;
  }

  public static float pitch(IMyTerminalBlock block)
  {
    if (block is IMyGyro)
    {
      return ((IMyGyro)block).Pitch;
    }

    return Single.NaN;
  }

  public static float roll(IMyTerminalBlock block)
  {
    if (block is IMyGyro)
    {
      return ((IMyGyro)block).Roll;
    }

    return Single.NaN;
  }
}

public class Thruster
{
  IMyThrust thruster;

  public Thruster(IMyThrust thruster)
  {
    this.thruster = thruster;
  }

  void setOverride(double newValue)
  {
    adjustToValue(thruster, newValue, "IncreaseOverride", "DecreaseOverride", ValueGetters.thrustOverride);
  }

  void disableOverride()
  {
    adjustToValue(thruster, 0, "IncreaseOverride", "DecreaseOverride", ValueGetters.thrustOverride);
  }
}

public class Gyro
{
  IMyGyro gyro;

  public Gyro(IMyGyro gyro)
  {
    this.gyro = gyro;
  }

  void enableOverride()
  {
    if (!gyro.GyroOverride)
    {
      applyAction(gyro, "Override");
    }
  }

  void disableOverride()
  {
    setYaw(0);
    setPitch(0);
    setRoll(0);

    if (gyro.GyroOverride)
    {
      applyAction(gyro, "Override");
    }
  }

  void setYaw(double newValue)
  {
    enableOverride();
    adjustToValue(gyro, newValue, "IncreaseYaw", "DecreaseYaw", ValueGetters.yaw);
  }

  void setPitch(double newValue)
  {
    enableOverride();
    adjustToValue(gyro, newValue, "IncreasePitch", "DecreasePitch", ValueGetters.pitch);
  }

  void setRoll(double newValue)
  {
    enableOverride();
    adjustToValue(gyro, newValue, "IncreaseRoll", "DecreaseRoll", ValueGetters.roll);
  }
}

List<IMyThrust> getAllThrusters()
{
  List<IMyTerminalBlock> allThrusters = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyThrust>(allThrusters);
  List<IMyThrust> res = new List<IMyThrust>();

  for (int i = 0; i < allThrusters.Count; ++i)
  {
    res.Add(allThrusters[i] as IMyThrust);
  }

  return res;
}

List<IMyThrust> getThrustersByDirection(string direction)
{
  direction = processDirectionName(direction);
  List<IMyTerminalBlock> allThrusters = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyThrust>(allThrusters);

  List<IMyThrust> res = new List<IMyThrust>();

  for (int i = 0; i < allThrusters.Count; ++i)
  {
    IMyThrust thruster = allThrusters[i] as IMyThrust;

    bool contains = thruster.CustomName.IndexOf("(" + direction + ")", StringComparison.OrdinalIgnoreCase) >= 0;

    if (contains)
    {
      res.Add(thruster);
    }

  }

  return res;
}

void stop()
{
  var thrusters = getAllThrusters();

  for (int i = 0; i < thrusters.Count; ++i)
  {
    new Thruster(thrusters[i]).disableOverride();
  }
}

bool isSensorActive(string name)
{
  return (getBlock(name) as IMySensorBlock).IsActive;
}

void setThrustDirection(string direction, int totalForce)
{
  direction = processDirectionName(direction);

  var thrusters = getThrustersByDirection(direction);

  // Seems like override is ignored if force is less than 450N
  int individualForce = Math.max(450, totalForce / i);

  for (int i = 0; i < thrusters.Count; ++i)
  {
    new Thruster(thrusters[i]).setOverride(individualForce);
  }
}

