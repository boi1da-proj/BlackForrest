window.addEventListener('DOMContentLoaded',()=>{
  const btn=document.getElementById('run');
  const out=document.getElementById('out');
  btn.onclick=async()=>{
    out.textContent='Running…';
    try{
      const res=await runShadow({module_path:'modules/compute_aabb/module.py',inputs:{points:[[0,0,0],[1,2,3],[-1,5,-2]]}});
      out.textContent=JSON.stringify(res,null,2);
    }catch(e){ out.textContent=String(e); }
  };
});

