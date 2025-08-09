#r "nuget: Newtonsoft.Json, 13.0.3"
using System; using System.IO; using System.Collections.Generic; using Newtonsoft.Json;
public class Payload{ public string module_path; public object inputs; }
// Inputs: P (List<Rhino.Geometry.Point3d>), Density (double)
void RunScript(List<Rhino.Geometry.Point3d> P, double Density, ref object Result){
  var payload = new Payload{ module_path = "modules/compute_aabb/module.py", inputs = new { points = P, density = Density } };
  var root = Directory.GetCurrentDirectory();
  var inp = Path.Combine(root, "shadow_in.json"); var outp = Path.Combine(root, "shadow_out.json");
  File.WriteAllText(inp, JsonConvert.SerializeObject(payload));
  var psi = new System.Diagnostics.ProcessStartInfo("python", $"runner/shadow_runner.py --config shadow/shadow_config.json --in {inp} --out {outp}");
  psi.WorkingDirectory = root; psi.RedirectStandardError=true; psi.RedirectStandardOutput=true; psi.UseShellExecute=false;
  var p = System.Diagnostics.Process.Start(psi); p.WaitForExit(60_000);
  Result = File.Exists(outp) ? File.ReadAllText(outp) : p.StandardError.ReadToEnd();
}

