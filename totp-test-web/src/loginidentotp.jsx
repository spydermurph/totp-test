import { useState } from "react";
import { useNavigate } from "react-router-dom";

const LoginIdenTOTP = () => {
  const [emailAddress, setEmailAddress] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    const blog = { emailAddress, password };

    try{
      const response = await fetch('https://localhost:44363/api/accounts/loginidentitytotp', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(blog)
      });
      if(response.status === 200) {
        let data = await response.json();
        console.log(data.message);
        if(data.isTwoFactor === true && data.tokenUri !== null){
          //navigate('Twofactor', {state:{email:data.emailAddress}});
          navigate('/registeridentotp', {state: {email:data.email, provider:data.provider, token:data.token, tokenuri:data.tokenuri}});
        }
        if(data.isRegistered) {
          console.log("Go to submit");
          navigate('/submitidentotp', {state: {email:data.email, provider:data.provider}});
        }
      } else if( response.status === 401) {
        alert("Invalid username or password");
      } else {
        alert("Unknown error");
      }
    } catch(err) {
      console.log(err);
      alert("Could not connect to server");
    }
    
    /*.then((response) => {
        return response.json();
    }).then((data) => {
        console.log(data);
        if(data.isTwoFactor === true && data.tokenUri !== null){
            //navigate('Twofactor', {state:{email:data.emailAddress}});
          navigate('/registeridentotp', {state: {email:data.email, provider:data.provider, token:data.token, tokenuri:data.tokenuri}});
        }
        if(data.isRegistered) {
          console.log("Go to submit");
          navigate('/submitidentotp', {state: {email:data.email, provider:data.provider}});
        }
    }).catch((error) => {
      console.log(error);
      alert("Could not connect to server");
    })*/
  }

  return (
    <div className="loginidentotp">
      <h2>Login Identity TOTP</h2>
      <form onSubmit={handleSubmit}>
        <label>Name:</label>
        <input 
          type="text" 
          required 
          value={emailAddress}
          onChange={(e) => setEmailAddress(e.target.value)}
        />
        <label>Password:</label>
        <input
          type="text"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        ></input>
        <button>Login</button>
      </form>
    </div>
  );
}
 
export default LoginIdenTOTP;