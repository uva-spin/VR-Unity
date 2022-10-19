<h1>3D Proton: Electomagnetic V1.0</h1>
<h2>What's Different?</h2>
<h3>Electric Charge</h3>
<p>All quarks and seaquarks are charged with either a +2/3 or -1/3 electric charge before they are created</p>
<p>The charge can be modified in any script of type <b>TypeQuark</b> by two boolean values 
<ul>
<li><b>IsCharged</b> tells Unity if the object has a charge or if it is uncharged (used by force probes)</li>
<li><b>Charge</b> tells Unity if the object has a +2/3 charge (<b>true</b>) or a -1/3 charge (<b>false</b>)</li>
</ul>

<h3>TypeQuark</h3>
<p><b>Quark</b> has been modified to be an extension of <b>TypeQuark</b> to standardize all quarks and make EM calculations easier to understand</p>
<p>The new <b>QuarkPair</b> script is used by individual quarks contained within seaquarks and is also an extension of <b>TypeQuark</b></p>
<p>Force probes have also been added to calculate the EM force at any point placed via the Unity editor and cannot contribute to EM forces</p>
<p>The force probe prefab can be accessed under Prefabs > Models</p>

<h3>Electromagnetic Fields</h3>
<p>Charged <b>TypeQuark</b> particles contribute to EM fields within the proton</p>
<p>All calculations are handled by the proton in the script <b>EMField</b></p>
<p>Several variables have also been added to modify EM calculations and each have associated tooltips to tell you more about them</p>
<ul>
<li>U0 Pi is the physics constant for the permittivity of free space divided by 4 pi (1E-7) used in the magnetic field calculation</li>
<li>K is the electric k constant used in the electric field calculation</li>
<li>Scale scales down the size of the proton down to around the actual size of the proton</li>
<li>C Scale converts the charges to Coulombs</li>
<li>V Scale increases the power velocity contributes to the magnetic field calculation (velocity^2)</li>
<li>Strength multiplies the total force calculated (F = strength * (F from E + F from B))</li>
<li>Sea Strength modifies the strength EM fields contribute to non-physics based quarks (Broken)</li>
<li>Fast field skips magnetic field calculations (Should only be used if magnetic field calculations are close to 0)</li>
<li>Physics Quarks enables non-physics based seaquarks (Broken)</li>
<li>Sea Lines are unused and force lines for seaquarks are instead handled by the All Quark Script</li>
<li>Cancel Threshold is the minimum distance (in-editor units) of which forces are not calculated between quarks to prevent /0 exceptions and will automatically cancel each other out if both quarks are seaquarks of opposite color types</li>
</ul>
<p>The <b>EMField</b> script can be disabled from the editor without error if needed, including during runtime</p>

<h3>1/r Centripetal Force</h3>
<p>A centripetal force is applied to all <b>TypeQuark</b> with a Rigidbody (accepts Physics calculations) towards the center of the proton at all times</p>
<p>The strength of this force can be modified by the Centripetal Force slider in the menu</p>

<h3>Sea Quark Spawn Changes</h3>
<p>The proton now requires a certain amount of energy before creating a new seaquark</p>
<p>Creating a new pair reduces the available energy by 1</p>
<p>Deleting a seaquark by reducing its HP to 0 will increase the available energy by 1</p>
<p>Seaquarks will also automatically be deleted if they reach 100 units away from the proton</p>

<h3>Parton Spin Sum (WIP)</h3>
<p>A work in progress feature still being prototyped</p>
<p>This script will take 2 passes through each <b>TypeQuark</b> and assign either a spin up or spin down value based off of its angular momentum and the angular momentum of other <b>TypeQuarks</b></p>
<p>Currently only works along the z-axis and spins cannot be in any direction other than up or down</p>
<p>Also currently attempts to sum spins to 0 instead of 1/2</p>
<p>Unlike EM, this has not yet been properly scaled</p>
<p>If enabled, the console will output some information on the calculated spin in 5 chunks separated by |'s:</p>
<ol>
<li><b>Total spin</b> calculated via Σ(spin * angular momentum) = Σ(s * m(v x R)) (mass not yet multiplied)</li>
<li><b>Number</b> of quarks with spin up (<b># ^</b>), and spin down (<b># v</b>)</li>
<li><b>Highest contributing</b> quark to total</li>
<li><b>Average</b> total spin during runtime</li>
<li><b>Greatest</b> total spin during runtime</li>
</ol>
<p>Can be disabled without error, including during runtime</p>

<h3>Visuals</h3>
<p>Forcelines have been added to each <b>TypeQuark</b> to visualize the direction and strength of EM forces on the object</p>
<p>These forcelines can be disabled from the menu by pressing Toggle Lines</p>
<p>Quark visuals and color have been moved to <b>TypeQuark</b></p>
<p>An FPS counter has also been added to the menu</p>

<h3>Music</h3>
<p>A music player and track has been added in the Scene Hierarchy at the top</p>
<p>The track will be turned off by default and can be enabled by checking Auto Play before runtime</p>
